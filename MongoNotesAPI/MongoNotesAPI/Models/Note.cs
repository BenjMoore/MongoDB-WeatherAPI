using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoNotesAPI.Models
{
    public class Note
    {
        [JsonIgnore]
        [BsonId]
        public ObjectId _id { get; set; }
        public string ObjId => _id.ToString();
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
    }
}

