using ICTPRG553.Models.DTOs;
using ICTPRG553.Models.Filters;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.DTOs;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Services;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MongoNotesAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        //Stores the reference to our mongo collection so we can use it in our code.
        private readonly IMongoCollection<ApiUser> _users;
        //Request the mongo conneciton builder class from our dependency injection
        public UserRepository(MongoConnectionBuilder builder)
        {
            //Use the builder to create a connection to our Users collection and
            //store the collection in the readonly field.
            _users = builder.GetDatabase().GetCollection<ApiUser>("Users");
        }
        public OperationResponseDTO<ApiUser> DeleteMany(UserFilter userFilter)
        {
            // Passes the provided filter to the method that will build a set of MongoDB filter definitions.
            var filter = GenerateFilterDefinition(userFilter);

            // Send the delete request to MongoDB to delete all entries matching the filter
            var result = _users.DeleteMany(filter);

            // Check if any records were deleted by MongoDB and send back details regarding the success/failure of the changes
            if (result.DeletedCount > 0)
            {
                return new OperationResponseDTO<ApiUser>
                {
                    Message = "Users Deleted Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<ApiUser>
                {
                    Message = "No Users deleted. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public OperationResponseDTO<ApiUser> UpdateRole(UserRoleUpdateDTO details)
        {
            var filter = Builders<ApiUser>.Filter;

            // Create a filter to match users within the specified date range
            var dateFilter = filter.And(
                filter.Gte(user => user.Created, details.createdAfter),
                filter.Lte(user => user.Created, details.createdBefore)
            );

            // Create an update definition to set the new role
            var update = Builders<ApiUser>.Update.Set(user => user.Role, details.Role);

            // Perform the update
            var result = _users.UpdateMany(dateFilter, update);

            if (result.ModifiedCount > 0)
            {
                return new OperationResponseDTO<ApiUser>
                {
                    Message = "Roles Updated Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<ApiUser>
                {
                    Message = "No Roles updated. Please check date range and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public ApiUser AuthenticateUser(string apiKey, UserRoles requiredRole)
        {
            //Create a filter to check the user collection for a match on the
            //apiKey field.
            var filter = Builders<ApiUser>.Filter.Eq(c => c.ApiKey, apiKey);
            //Add a second filter check to see if the user is active. The &= operator
            //adds this filter onto the previous filter set.
            filter &= Builders<ApiUser>.Filter.Eq(c => c.Active, true);

            //Check the database to see iof there is a current user with the specified role.
            var user = _users.Find(filter).FirstOrDefault();
            //If no user was found or there role does not have the required access level
            //specified in the parameters, return null.
            if (user == null || !IsAllowedRole(user.Role, requiredRole)) 
            {
                return null;
            }
            //If the user is authenticated, return their details.
            return user;
        }

        public bool CreateUser(ApiUser user)
        {
            //Create a filter to check the user collection for a match on the
            //email field.
            var filter = Builders<ApiUser>.Filter.Eq(c => c.Email, user.Email);
            //Pass the filter to mongo and see if any records match the provided
            //filter parameters.
            var existingUser = _users.Find(filter).FirstOrDefault();
            //If an existing user was found(meaingin the email is already registered)
            //return false
            if (existingUser != null) 
            {
                return false;
            }

            //Generate a GUID (Global unique identifier) to act as the apiKey for
            //the new user
            user.ApiKey = Guid.NewGuid().ToString();
            //Set the last access to the current datetime
            user.LastAccess = DateTime.Now;
            //Insert the new user into MongoDB
            _users.InsertOne(user);

            //Return true to confirm new user has been added.
            return true;

        }

        public bool DeleteUser(ApiUser user, string id)
        {
            ObjectId objId = ObjectId.Parse(id);
          
            // find a person using an equality filter on its id
            var filter = Builders<ApiUser>.Filter.Eq(e => e._id, objId);

            // delete the person
            var targetedUser = _users.DeleteOne(filter);

            if (targetedUser.DeletedCount == 1)
            {
                return true;
            }

           else
            {
                return false;
            }
        }
        private FilterDefinition<ApiUser> GenerateFilterDefinition(UserFilter userFilter)
        {
            //Requests a filter builder for the Note model from the builders class
            var builder = Builders<ApiUser>.Filter;
            //Uses the filter builder to create an empty filter(no filter options)
            var filter = builder.Empty;

         
            if (userFilter.CreatedBefore != null)
            {
                //Creates a Less than or equal to filter that checks the created date of the note against the
                //Created before date of the noteFilter
                filter &= builder.Lte(data => data.LastAccess, userFilter.CreatedBefore.Value);
            }
            if (userFilter.CreatedAfter != null)
            {
                //Creates a greater than or equal to filter that checks the created date of the note against the
                //Created after date of the noteFilter
                filter &= builder.Gte(data => data.LastAccess, userFilter.CreatedAfter.Value);
            }

            //Returns the completed filter definitions to the caller.
            return filter;
        }
        private UpdateDefinition<ApiUser> GenerateUpdateDefinition(UserRoleUpdateDTO details)
        {
            //Creates a filter builder to allow us to build update rules in a way that
            ////allows us to append extra rules to them afterwards.
            var builder = Builders<ApiUser>.Update.Combine();
            //Declare an update definition object which starts as null.
            UpdateDefinition<ApiUser> updateRules = null;

            //If the title field of the details is not empty, create a new rule for updating the
            
            //If the title field of the details is not empty, create a new rule for updating the
            //title property in the notes rules.
           
                //Check if I have any update rules set yet, if not use the builder to create
                //a new definitions object and set its first rule.
                if (updateRules == null)
                {
                    updateRules = builder.Set(data => data.Role, details.Role);
                }
                //If there is already at least one exisiting rule, add a new one to the set. 
                else
                {
                    updateRules = updateRules.Set(data => data.Role, details.Role);
                }
            

            //Returns the completed update definitions/rules to the caller 
            return updateRules;
        }

        public void UpdateLastLogin(string apiKey)
        {
            //Create a filter to check the user collection for a match on the
            //apiKey field.
            var filter = Builders<ApiUser>.Filter.Eq(c => c.ApiKey, apiKey);
            //Create an update rule to change the last access field to the current datetime
            var update = Builders<ApiUser>.Update.Set(u => u.LastAccess, DateTime.Now);
            //Pass the filter and update rule to MongoDb to find the desired record and
            //update its last access value.
            _users.UpdateOne(filter, update);
        }

        private bool IsAllowedRole(string userRole, UserRoles requiredRole)
        {
            //Check the provided role to see if it matches one of the specified roles in
            //our enum
            if (!Enum.TryParse(userRole.ToUpper(), out UserRoles roleName))
            {
                //If it can;t be matched, return false to inform there is a problem.
                return false;
            }

            int userRoleNumber = (int)roleName;
            int requiredRoleNumber  = (int)requiredRole;

            return userRoleNumber <= requiredRoleNumber;
        }
    }
}
