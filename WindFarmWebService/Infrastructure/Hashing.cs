using System;

namespace WindFarmWebService.Infrastructure
{
    public static class Hashing
    {
        public static long GetPartitionKey(string windFarm)
        {
            if (string.IsNullOrWhiteSpace(windFarm)) throw new ArgumentException($"{windFarm}");

            var partitionKey = (long)windFarm[0] % 2;
            return partitionKey;
        }
    }
}
