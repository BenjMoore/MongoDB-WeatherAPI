using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ICTPRG553.Models.Filters
{
    public class TimeFilter
    {
        public DateTime? LastAccess { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
    }
}


