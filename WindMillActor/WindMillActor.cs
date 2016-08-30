using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace WindMillActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class WindMillActor : Actor, IWindMillActor
    {
        private string _stateName = "measurements";

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"{nameof(WindMillActor)} activated");
            await this.StateManager.TryAddStateAsync(_stateName, new List<WindMeasurement>());
            await base.OnActivateAsync();
        }

        async Task IWindMillActor.NewMeasurement(WindMeasurement windMeasurement)
        {
            var windMeasurements = await this.StateManager.GetStateAsync<List<WindMeasurement>>(_stateName);
            windMeasurements.Add(windMeasurement);
            await this.StateManager.SetStateAsync(_stateName, windMeasurements);
        }
    }
}
