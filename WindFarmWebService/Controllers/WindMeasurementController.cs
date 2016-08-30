using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json.Linq;
using StorageService;
using StorageService.Models;
using WindFarmWebService.Infrastructure;
using WindFarmWebService.Models;
using WindMillActor;

namespace WindFarmWebService.Controllers
{
    public class WindMeasurementController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]WindMeasurementInputModel model)
        {
            try
            {
                var windMillActor = ActorProxy.Create<IWindMillActor>(new ActorId($"{model.WindFarm}-{model.WindMill}"));
                var windMeasurement = new WindMeasurement(model.WindSpeed, model.TimeOfMeasurement, model.WindDirection);
                await windMillActor.NewMeasurement(windMeasurement);

                var storageServiceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/StorageService");
                var partitionKey = Hashing.GetPartitionKey(model.WindFarm);
                var storageService = ServiceProxy.Create<IStorageService>(storageServiceUri, new ServicePartitionKey(partitionKey));
                var measurementData = new WindMeasurementData(model.WindFarm, model.WindMill, model.WindSpeed, model.TimeOfMeasurement, model.WindDirection);
                await storageService.StoreWindMeasurementsAsync(measurementData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                var windMeasurementViewModels = new List<WindMeasurementViewModel>();
                var storageServiceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/StorageService");
                foreach(var partition in await this.GetServicePartitionKeysAsync())
                {
                    // http communication
                    //var servicePartitionKey = new ServicePartitionKey(partition.LowKey);
                    //var servicePartitionResolver = ServicePartitionResolver.GetDefault();
                    //ResolvedServicePartition resolvedPartition = await servicePartitionResolver.ResolveAsync(storageServiceUri, servicePartitionKey, CancellationToken.None);
                    //ResolvedServiceEndpoint ep = resolvedPartition.GetEndpoint();

                    //JObject addresses = JObject.Parse(ep.Address);
                    //string primaryReplicaAddress = (string)addresses["Endpoints"].First();

                    //UriBuilder primaryReplicaUriBuilder = new UriBuilder(primaryReplicaAddress);

                    //var httpClient = new HttpClient();
                    //string result = await httpClient.GetStringAsync(primaryReplicaUriBuilder.Uri);


                    var storageService = ServiceProxy.Create<IStorageService>(storageServiceUri, new ServicePartitionKey(partition.LowKey));
                    var windMeasurements = await storageService.GetWindMeasurementsAsync();
                    var viewModels = windMeasurements.Select(
                        x => new WindMeasurementViewModel(x.WindFarm, x.WindMill, x.WindSpeed, x.TimeOfMeasurement, x.WindDirection));
                    windMeasurementViewModels.AddRange(viewModels);
                }
                return Request.CreateResponse(HttpStatusCode.OK, windMeasurementViewModels);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        private async Task<IList<Int64RangePartitionInformation>> GetServicePartitionKeysAsync()
        {
            // Get the list of partitions up and running in the service.
            var fabricClient = new FabricClient();
            var storageServiceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/StorageService");

            ServicePartitionList partitionList = await fabricClient.QueryManager.GetPartitionListAsync(storageServiceUri);

            // For each partition, build a service partition client used to resolve the low key served by the partition.
            IList<Int64RangePartitionInformation> partitionKeys =
                new List<Int64RangePartitionInformation>(partitionList.Count);
            foreach (Partition partition in partitionList)
            {
                Int64RangePartitionInformation partitionInfo =
                    partition.PartitionInformation as Int64RangePartitionInformation;
                if (partitionInfo == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "The service {0} should have a uniform Int64 partition. Instead: {1}",
                            storageServiceUri,
                            partition.PartitionInformation.Kind));
                }

                partitionKeys.Add(partitionInfo);
            }

            return partitionKeys;
        }
    }
}
