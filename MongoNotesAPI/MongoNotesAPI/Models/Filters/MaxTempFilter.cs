
using MongoDB.Bson;

namespace MongoNotesAPI.Models.Filters
{
    public class MaxTempFilter
    {
    
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

    }
}
