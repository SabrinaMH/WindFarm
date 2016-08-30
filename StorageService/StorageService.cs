using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using StorageService.Models;

namespace StorageService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public sealed class StorageService : StatefulService, IStorageService
    {
        public StorageService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            //used for http communication
            //return new[] {new ServiceReplicaListener(this.CreateInternalListener)};
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

        public async Task StoreWindMeasurementsAsync(WindMeasurementData windMeasurement)
        {
            using (var tx = this.StateManager.CreateTransaction())
            {
                var measurementDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, WindMeasurementData>>("windMeasurementDictionary");
                await measurementDictionary.AddAsync(tx, windMeasurement.WindFarm, windMeasurement);
                await tx.CommitAsync();
            }
        }

        public async Task<List<WindMeasurementData>> GetWindMeasurementsAsync()
        {
            var measurements = new List<WindMeasurementData>();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var measurementDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, WindMeasurementData>>("windMeasurementDictionary");
                var measurementEnumerable = await measurementDictionary.CreateEnumerableAsync(tx);
                using (var enumerator = measurementEnumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        measurements.Add(enumerator.Current.Value);
                    }
                }
                return measurements;
            }
        }


        #region Http communication
        // Called once for each partition
        private ICommunicationListener CreateInternalListener(ServiceContext context)
        {
            // Partition replica's URL is the node's IP, port, PartitionId, ReplicaId, Guid
            EndpointResourceDescription internalEndpoint = context.CodePackageActivationContext.GetEndpoint("StorageServiceEndpoint");

            // Multiple replicas of this service may be hosted on the same machine,
            // so this address needs to be unique to the replica which is why we have partition ID + replica ID in the URL.
            // HttpListener can listen on multiple addresses on the same port as long as the URL prefix is unique.
            // The extra GUID is there for an advanced case where secondary replicas also listen for read-only requests.
            // When that's the case, we want to make sure that a new unique address is used when transitioning from primary to secondary
            // to force clients to re-resolve the address.
            // '+' is used as the address here so that the replica listens on all available hosts (IP, FQDM, localhost, etc.)

            string uriPrefix = String.Format(
                "{0}://+:{1}/{2}/{3}-{4}/",
                internalEndpoint.Protocol,
                internalEndpoint.Port,
                context.PartitionId,
                context.ReplicaOrInstanceId,
                Guid.NewGuid());

            string nodeIP = FabricRuntime.GetNodeContext().IPAddressOrFQDN;

            // The published URL is slightly different from the listening URL prefix.
            // The listening URL is given to HttpListener.
            // The published URL is the URL that is published to the Service Fabric Naming Service,
            // which is used for service discovery. Clients will ask for this address through that discovery service.
            // The address that clients get needs to have the actual IP or FQDN of the node in order to connect,
            // so we need to replace '+' with the node's IP or FQDN.
            string uriPublished = uriPrefix.Replace("+", nodeIP);
            return new HttpCommunicationListener(uriPrefix, uriPublished, this.ProcessInternalRequest);
        }



        private async Task ProcessInternalRequest(HttpListenerContext context, CancellationToken cancelRequest)
        {
            List<WindMeasurementData> output = null;

            try
            {
                output = await this.GetWindMeasurementsAsync();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }

            using (HttpListenerResponse response = context.Response)
            {
                if (output != null)
                {
                    byte[] outBytes = Encoding.UTF8.GetBytes("hej");
                    response.OutputStream.Write(outBytes, 0, outBytes.Length);
                }
            }
        }
        #endregion


    }
}
