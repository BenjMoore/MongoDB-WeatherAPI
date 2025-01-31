using ICTPRG553.Models.Filters;
using MongoDB.Bson;
using MongoNotesAPI.Models.Filters;

namespace ICTPRG553.Models.DTOs
{
    public class UserRoleUpdateDTO
    {
        public DateTime createdBefore { get; set; }
        public DateTime createdAfter { get; set; }

        public string Role { get; set; }

    }
}
