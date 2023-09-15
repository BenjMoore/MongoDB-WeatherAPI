using MongoDB.Bson;

namespace ICTPRG553.Models.DTOs
{
    public class PrecipitationDTO
    {
        public string deviceName { get; set; }
        public DateTime Time { get; set; }
        public Double Precipitation { get; set; }

    }
}
