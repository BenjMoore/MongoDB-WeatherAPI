using MongoNotesAPI.Models.Filters;

namespace MongoNotesAPI.Models
{
    public class WeatherPatchDetailsDTO
    {
        public WeatherFilter Filter { get; set; }
        public string deviceName { get; set; }
        public Double Precipitation { get; set; }
        public DateTime Time { get; set; }
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }
        public Double Temperature { get; set; }
        public Double atmosphericPressure { get; set; }
        public Double maxWindSpeed { get; set; }
        public Double solarRadiation { get; set; }
        public Double vaporPressure { get; set; }
        public Double Humidity { get; set; }
        public Double windDirection { get; set; }
    }
}