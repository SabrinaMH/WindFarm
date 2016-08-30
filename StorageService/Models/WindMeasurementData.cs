using System;
using System.Runtime.Serialization;

namespace StorageService.Models
{
    [DataContract]
    public class WindMeasurementData
    {
        public WindMeasurementData(string windFarm, int windMill, int windSpeed, DateTime timeOfMeasurement, string windDirection)
        {
            WindFarm = windFarm;
            WindMill = windMill;
            WindSpeed = windSpeed;
            TimeOfMeasurement = timeOfMeasurement;
            WindDirection = windDirection;
        }

        [DataMember]
        public string WindFarm { get; set; }
        [DataMember]
        public int WindMill { get; set; }
        [DataMember]
        public int WindSpeed { get; set; }
        [DataMember]
        public DateTime TimeOfMeasurement { get; set; }
        [DataMember]
        public string WindDirection { get; set; }
    }
}