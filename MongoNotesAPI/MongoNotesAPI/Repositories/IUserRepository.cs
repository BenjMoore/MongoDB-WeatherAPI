using ICTPRG553.Models.DTOs;
using ICTPRG553.Models.Filters;
using MongoNotesAPI.Models;

namespace MongoNotesAPI.Repositories
{
    public interface IUserRepository
    {
        bool CreateUser(ApiUser user);
        bool AuthenticateUser(string apiKey, params UserRoles[] allowedRoles);
        void UpdateLastLogin(string apiKey);
        bool DeleteUser(string id);
        public OperationResponseDTO<ApiUser> DeleteMany(UserFilter Filter);
        //public OperationResponseDTO<ApiUser> UpdateRole(string id, string role);
        public OperationResponseDTO<ApiUser> UpdateRole(UserRoleUpdateDTO details);
      
    }
}
