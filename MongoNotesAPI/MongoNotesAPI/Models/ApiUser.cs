using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoNotesAPI.Models
{
    public class ApiUser
    {
        //This aatribute tag flags this property as the primary key field when it is
        //stroed in your mongo collection.
        [BsonId]
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }

        //This attribute allows you to specify the name you want used in mongo
        //for this property
        [BsonElement("Last Access")]
        public DateTime LastAccess { get; set; }
        public DateTime Created { get; set; }
        public string ApiKey { get; set; }
    }
}
