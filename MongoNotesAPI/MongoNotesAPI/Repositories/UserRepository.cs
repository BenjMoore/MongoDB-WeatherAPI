using ICTPRG553.Models.DTOs;
using ICTPRG553.Models.Filters;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.DTOs;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Services;
using System.Data;
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

        public bool AuthenticateUser(string apiKey, params UserRoles[] allowedRoles)
        {
            //Create a filter to find any user who contain the provided api key.
            var filter = Builders<ApiUser>.Filter.Eq(u => u.ApiKey, apiKey);
            //Send the request to the databse to find any matching user that exists.
            var user = _users.Find(filter).FirstOrDefault();
            //If no user was found, return false to indicate the key was not valid.
            if (user == null)
            {
                return false;
            }
            //Check the user's role agains the allowed roles and return the result. 
            return HasRequiredRole(user.Role.ToUpper(), allowedRoles);
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

        public bool DeleteUser(string id)
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

       
        private bool HasRequiredRole(string userRole, UserRoles[] requiredRoles)
        {
            //Try to convert the userRole form a string to its Enum equivalent and
            //store the result oin the out parameter
            if (!Enum.TryParse(userRole, out UserRoles userRoleType))
            {
                //If it cannot be done (misspelt/incorrect word), return false
                return false;
            }
            //Cycle through each of the required roles and check each one against the 
            //user role enum.
            foreach (var role in requiredRoles)
            {
                //If a match is found, return true to indicate the check has passed.
                if (userRoleType.Equals(role))
                {
                    return true;
                }
            }
            //If no match was found, return false to indicate a failure.
            return false;
        }
        
    }
}
