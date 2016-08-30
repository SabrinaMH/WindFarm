using System;
using Microsoft.Build.Framework;

namespace WindFarmWebService.Models
{
    public class WindMeasurementInputModel
    {
        [Required]
        public string WindFarm { get; set; }
        [Required]
        public int WindMill { get; set; }
        [Required]
        public int WindSpeed { get; set; }
        [Required]
        public DateTime TimeOfMeasurement { get; set; }
        [Required]
        public string WindDirection { get; set; }
    }
}