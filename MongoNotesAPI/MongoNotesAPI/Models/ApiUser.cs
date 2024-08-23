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
        [BsonElement("Name")]
        public string Name { get; set; }
        [BsonElement("Email")]
        public string Email { get; set; }
        [BsonElement("Role")]
        public string Role { get; set; }
        [BsonElement("Active")]
        public bool Active { get; set; }

        //This attribute allows you to specify the name you want used in mongo
        //for this property
        [BsonElement("Last Access")]
        public DateTime LastAccess { get; set; }
        [BsonElement("Created")]
        public DateTime Created { get; set; }
        [BsonElement("ApiKey")]
        public string ApiKey { get; set; }
    }
}
