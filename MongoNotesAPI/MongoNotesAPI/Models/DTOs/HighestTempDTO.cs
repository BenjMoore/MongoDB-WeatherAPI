using MongoDB.Bson;

namespace ICTPRG553.Models.DTOs
{
    public class PrecipitationTempDTO
    {
        public string deviceName { get; set; }
        public DateTime Time { get; set; }
        public Double Temperature { get; set; }
        public ObjectId _id { get; set; }

    }
}
