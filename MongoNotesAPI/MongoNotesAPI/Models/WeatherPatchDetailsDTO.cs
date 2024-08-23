using MongoDB.Bson.Serialization.Attributes;
using MongoNotesAPI.Models.Filters;

namespace MongoNotesAPI.Models
{
    public class WeatherPatchDetailsDTO
    {
        public WeatherFilter Filter { get; set; }
        public string deviceName { get; set; }
        [BsonElement("Precipitation")]
        public Double Precipitation { get; set; }
        [BsonElement("Time")]
        public DateTime Time { get; set; }
        [BsonElement("Latitude")]
        public Double Latitude { get; set; }
        [BsonElement("Longitude")]
        public Double Longitude { get; set; }
        [BsonElement("Temperature")]
        public Double Temperature { get; set; }
        [BsonElement("atmosphericPressure")]
        public Double atmosphericPressure { get; set; }
        [BsonElement("maxWindSpeed")]
        public Double maxWindSpeed { get; set; }
        [BsonElement("solarRadiation")]
        public Double solarRadiation { get; set; }
        [BsonElement("vaporPressure")]
        public Double vaporPressure { get; set; }
        [BsonElement("Humidity")]
        public Double Humidity { get; set; }
        [BsonElement("windDirection")]
        public Double windDirection { get; set; }
    }
}