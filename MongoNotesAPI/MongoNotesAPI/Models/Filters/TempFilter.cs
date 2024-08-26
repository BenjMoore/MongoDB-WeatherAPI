using MongoDB.Bson.Serialization.Attributes;

namespace ICTPRG553.Models.Filters
{
    public class TempFilter
    {
        [BsonElement("Temperature (°C)")]
        public Double Temperature { get; set; }

        [BsonElement("Device Name")]
        public string deviceName { get; set; }

        [BsonElement("Time")]
        public DateTime Time {  get; set; }
    }
}
