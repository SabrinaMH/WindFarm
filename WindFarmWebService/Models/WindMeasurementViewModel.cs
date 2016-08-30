using System;

namespace WindFarmWebService.Models
{
    public class WindMeasurementViewModel
    {
        public string WindFarm { get; set; }
        public int WindMill { get; set; }
        public int WindSpeed { get; set; }
        public DateTime TimeOfMeasurement { get; set; }
        public string WindDirection { get; set; }

        public WindMeasurementViewModel(string windFarm, int windMill, int windSpeed, DateTime timeOfMeasurement, string windDirection)
        {
            WindFarm = windFarm;
            WindMill = windMill;
            WindSpeed = windSpeed;
            TimeOfMeasurement = timeOfMeasurement;
            WindDirection = windDirection;
        }
    }
}