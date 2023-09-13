using Amazon.Runtime.SharedInterfaces;
using MongoDB.Bson;

namespace MongoNotesAPI.Models.Filters
{
    public class WeatherFilter
    {
        public string deviceName { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        
    }
}
