using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ICTPRG553.Models.Filters
{
        public class UserFilter
        {
            //This aatribute tag flags this property as the primary key field when it is
            //stroed in your mongo collection.
            public ObjectId _id { get; set; }
            public string Name { get; set; }
            public DateTime? LastAccess { get; set; }
            public DateTime? CreatedBefore { get; set; }
            public DateTime? CreatedAfter { get; set; }
            }
    }


