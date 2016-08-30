using System;
using System.Runtime.Serialization;

namespace WindMillActor
{
    [DataContract]
    public class WindMeasurement
    {
        public int WindSpeed { get; set; }
        public DateTime TimeOfMeasurement { get; set; }
        public string WindDirection { get; set; }

        public WindMeasurement(int windSpeed, DateTime timeOfMeasurement, string windDirection)
        {
            WindSpeed = windSpeed;
            TimeOfMeasurement = timeOfMeasurement;
            WindDirection = windDirection;
        }
    }
}