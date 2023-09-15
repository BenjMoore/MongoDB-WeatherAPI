using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ICTPRG553.Models.Filters
{
        public class UserFilter
        {
            //This aatribute tag flags this property as the primary key field when it is
            //stroed in your mongo collection.
            public ObjectId _id { get; set; }// Not Needed
            public string Name { get; set; }// Not Needed
            public DateTime? LastAccess { get; set; } // Not Needed
            public DateTime? CreatedBefore { get; set; }
            public DateTime? CreatedAfter { get; set; }
            }
    }


