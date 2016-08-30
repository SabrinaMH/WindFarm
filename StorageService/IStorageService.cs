using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using StorageService.Models;

namespace StorageService
{
    public interface IStorageService : IService
    {
        Task StoreWindMeasurementsAsync(WindMeasurementData windMeasurement);
        Task<List<WindMeasurementData>> GetWindMeasurementsAsync();
    }
}