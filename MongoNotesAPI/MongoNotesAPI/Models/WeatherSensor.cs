using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoNotesAPI.Models
{
    public class WeatherSensor
    {
        [JsonIgnore]
        [BsonId]
        public ObjectId _id { get; set; }
        public string ObjId => _id.ToString();
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

