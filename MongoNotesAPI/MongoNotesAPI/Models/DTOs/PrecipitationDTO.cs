using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ICTPRG553.Models.DTOs
{
    public class PrecipitationDTO
    {
        [BsonElement("Device Name")]
        public string deviceName { get; set; }
        [BsonElement("Time")]
        public DateTime Time { get; set; }
        [BsonElement("Precipitation mm/h")]
        public Double Precipitation { get; set; }

    }
}