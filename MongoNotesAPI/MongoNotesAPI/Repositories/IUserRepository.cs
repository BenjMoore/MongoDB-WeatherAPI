using MongoNotesAPI.Models;

namespace MongoNotesAPI.Repositories
{
    public interface IUserRepository
    {
        bool CreateUser(ApiUser user);
        ApiUser AuthenticateUser(string apiKey, UserRoles requiredRole);
        void UpdateLastLogin(string apiKey);
        bool DeleteUser(ApiUser user, string id);
    }
}
