using ICTPRG553.Models.Filters;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Services;
using System.Text.RegularExpressions;

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

            if (String.IsNullOrEmpty(userFilter.Name) == false)
            {
                //Cleans the original string to remove any charactes that might cause issues with our
                //regex filter by escaping them(like'\n') out in the string.
                var cleanedString = Regex.Escape(userFilter.Name);

                //Adds a filter to the current filter set. This filter is a contains filter to find if the
                //specified field contains the provided string
                filter &= builder.Regex(data => data.Name, BsonRegularExpression.Create(cleanedString));
            }
            /*
            if (String.IsNullOrEmpty(weatherFilter.id) == false)
            {
                //Cleans the original string to remove any charactes that might cause issues with our
                //regex filter by escaping them(like'\n') out in the string.
                var cleanedString = Regex.Escape(weatherFilter.BodyMatch);
                //Adds a filter to the current filter set. This filter is a contains filter to find if the
                //specified field contains the provided string
                filter &= builder.Regex(data => data.Precipitation, BsonRegularExpression.Create(cleanedString));
            }*/
            if (userFilter.CreatedBefore != null)
            {
                //Creates a Less than or equal to filter that checks the created date of the note against the
                //Created before date of the noteFilter
                filter &= builder.Lte(data => data.LastAccess, userFilter.LastAccess.Value);
            }
            if (userFilter.CreatedAfter != null)
            {
                //Creates a greater than or equal to filter that checks the created date of the note against the
                //Created after date of the noteFilter
                filter &= builder.Gte(data => data.LastAccess, userFilter.LastAccess.Value);
            }

            //Returns the completed filter definitions to the caller.
            return filter;
        }

        public OperationResponseDTO<ApiUser> DeleteMany(UserFilter Filter)
        {
            //Passes the provided filter to the method that will build a set of mongo db
            //filter definitions.
            var filter = GenerateFilterDefinition(Filter);
            //Sends the delete request to MongoDB to delete all entries matching the filter
            var result = _users.DeleteMany(filter);

            //Check if any records were deleted by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.DeletedCount > 0)
            {
                return new OperationResponseDTO<ApiUser>
                {
                    Message = "User/s Deleted Successfully",
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
