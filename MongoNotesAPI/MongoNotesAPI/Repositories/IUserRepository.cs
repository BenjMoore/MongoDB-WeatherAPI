using ICTPRG553.Models.DTOs;
using ICTPRG553.Models.Filters;
using MongoNotesAPI.Models;

namespace MongoNotesAPI.Repositories
{
    public interface IUserRepository
    {
        bool CreateUser(ApiUser user);
        ApiUser AuthenticateUser(string apiKey, UserRoles requiredRole);
        void UpdateLastLogin(string apiKey);
        bool DeleteUser(ApiUser user, string id);
        public OperationResponseDTO<ApiUser> DeleteMany(UserFilter Filter);
        //public OperationResponseDTO<ApiUser> UpdateRole(string id, string role);
        public OperationResponseDTO<ApiUser> UpdateRole(UserRoleUpdateDTO details);
        object AuthenticateUser(string providedKey);
    }
}
