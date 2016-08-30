using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace WindMillActor
{
    public interface IWindMillActor : IActor
    {
        Task NewMeasurement(WindMeasurement windMeasurement);
    }
}
