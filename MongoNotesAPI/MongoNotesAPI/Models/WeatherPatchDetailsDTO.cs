using MongoDB.Bson.Serialization.Attributes;
using MongoNotesAPI.Models.Filters;

namespace MongoNotesAPI.Models
{
    public class WeatherPatchDetailsDTO
    {
        public WeatherFilter Filter { get; set; }
        [BsonElement("Device Name")]
        public string deviceName { get; set; }
        [BsonElement("Precipitation mm/h")]
        public Double Precipitation { get; set; }
        [BsonElement("Time")]
        public DateTime Time { get; set; }
        [BsonElement("Latitude")]
        public Double Latitude { get; set; }
        [BsonElement("Longitude")]
        public Double Longitude { get; set; }
        [BsonElement("Temperature (°C)")]
        public Double Temperature { get; set; }
        [BsonElement("Atmospheric Pressure (kPa)")]
        public Double atmosphericPressure { get; set; }
        [BsonElement("maxWindSpeed")]
        public Double maxWindSpeed { get; set; }
        [BsonElement("Solar Radiation (W/m2)")]
        public Double solarRadiation { get; set; }
        [BsonElement("Vapor Pressure (kPa)")]
        public Double vaporPressure { get; set; }
        [BsonElement("Humidity (%)")]
        public Double Humidity { get; set; }
        [BsonElement("Wind Direction (°)")]
        public Double windDirection { get; set; }
    }
}