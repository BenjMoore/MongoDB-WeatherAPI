using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Services;
using System.Text.RegularExpressions;

namespace MongoNotesAPI.Repositories
{
    public class WeatherRepository : IWeatherRepository
    {
        //Creating a readonly variable to hold our note collection details 
        private readonly IMongoCollection<WeatherSensor> _data;
        
        //Ask for the MongoConnectionBuilder from the dependency injection by requesting
        //it in the parameters of the constructor.
        public WeatherRepository(MongoConnectionBuilder connection)
        {
            //Use the connection to access the database and find the collection
            //called notes. The passing of the Note model into this method indicates that
            //the documents of the collection should be mapped to note objects.
            _data = connection.GetDatabase().GetCollection<WeatherSensor>("WeatherData");
        }


        public void Create(WeatherSensor createdReading)
        {
            _data.InsertOne(createdReading);
        }

        public void CreateMany(List<WeatherSensor> readingList)
        {
            _data.InsertMany(readingList);
        }

        public OperationResponseDTO<WeatherSensor> Delete(string id)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<WeatherSensor>.Filter.Eq(data => data._id, objId);
            //Use the filter to find the required record and delete it.
            var result = _data.DeleteOne(filter);

            //Check if any records were deleted by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.DeletedCount > 0)
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "Reading Deleted Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "No Readings deleted. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public OperationResponseDTO<WeatherSensor> DeleteMany(WeatherFilter weatherFilter)
        {
            //Passes the provided filter to the method that will build a set of mongo db
            //filter definitions.
            var filter = GenerateFilterDefinition(weatherFilter);
            //Sends the delete request to MongoDB to delete all entries matching the filter
            var result = _data.DeleteMany(filter);

            //Check if any records were deleted by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.DeletedCount > 0)
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "Reading/s Deleted Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "No readings deleted. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public IEnumerable<WeatherSensor> GetAll()
        {
            //Requests a filter builder for the Note model from the builders class
            var builder = Builders<WeatherSensor>.Filter;
            //Uses the filter builder to create an empty filter(no filter options)
            var filter = builder.Empty;
            //Sends the find request to MongoDB to return all entries matching the filter
            //which in this case will be every entry and then put them in a collection
            return _data.Find(filter).ToEnumerable();
        }

        public IEnumerable<WeatherSensor> GetAll(WeatherFilter weatherFilter)
        {
            //Passes the provided filter to the method that will build a set of mongo db
            //filter definitions.
            var filter = GenerateFilterDefinition(weatherFilter);

            //Sends the find request to MongoDB to return all entries matching the filter
            //which in this case will be every entry and then put them in a collection
            return _data.Find(filter).ToEnumerable();
        }

        public WeatherSensor GetById(string id)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<WeatherSensor>.Filter.Eq(data => data._id, objId);
            //Call the find method using the filter to find the first recod that matches.
            return _data.Find(filter).FirstOrDefault();
        }

        public OperationResponseDTO<WeatherSensor> Update(string id, WeatherSensor updatedReading)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<WeatherSensor>.Filter.Eq(data => data._id, objId);

            //Create an update builder to allow the creation of our set of update
            //definitions.
            var builder = Builders<WeatherSensor>.Update;
            //Define a set of update definitions(rules) to outline what fields need
            //to be updated and what they need to be updated to.
            var update = builder.Set(data => data.deviceName, updatedReading.deviceName)
                                .Set(data => data.Precipitation, updatedReading.Precipitation);

            //Call the update method and give it the filter to find the desired entry as 
            //well as the update definitions of what fields need to be changed.
            var result = _data.UpdateOne(filter, update);

            //Check if any records were chnaged by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.ModifiedCount > 0)
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "Reading Updated Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "No readings updated. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public OperationResponseDTO<WeatherSensor> UpdateMany(WeatherPatchDetailsDTO details)
        {
            //Passes the provided filter parameters to the method to build the filter rules
            //for mongo db.
            var filterDefinitions = GenerateFilterDefinition(details.Filter);
            //Passes the provided details parameters to the method to build the update rules
            //for mongo db.
            var updateDefinitions = GenerateUpdateDefinition(details);
            //Pass the filter and update rules to mongo db to process our request.
            var result = _data.UpdateMany(filterDefinitions, updateDefinitions);

            //Check if any records were chnaged by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.ModifiedCount > 0)
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "Reading/s Updated Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<WeatherSensor>
                {
                    Message = "No reading updated. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        //This is an alternate variation of the above upsate mthod which does a full
        //find and replace of the note object.
        public void Replace(string id, WeatherSensor updatedReading)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<WeatherSensor>.Filter.Eq(data => data._id, objId);
            //Pass the converted object Id into the updated note
            updatedReading._id = objId;
            //Call the replace method and give it the filter to find the desired entry as 
            //well as the updated note details that it will be replaced with.
            _data.ReplaceOne(filter, updatedReading);
        }

        private FilterDefinition<WeatherSensor> GenerateFilterDefinition(WeatherFilter noteFilter)
        {
            //Requests a filter builder for the Note model from the builders class
            var builder = Builders<WeatherSensor>.Filter;
            //Uses the filter builder to create an empty filter(no filter options)
            var filter = builder.Empty;

            if (String.IsNullOrEmpty(noteFilter.TitleMatch) == false)
            {
                //Cleans the original string to remove any charactes that might cause issues with our
                //regex filter by escaping them(like'\n') out in the string.
                var cleanedString = Regex.Escape(noteFilter.TitleMatch);
                //Adds a filter to the current filter set. This filter is a contains filter to find if the
                //specified field contains the provided string
                filter &= builder.Regex(data => data.deviceName, BsonRegularExpression.Create(cleanedString));
            }
            if (String.IsNullOrEmpty(noteFilter.BodyMatch) == false)
            {
                //Cleans the original string to remove any charactes that might cause issues with our
                //regex filter by escaping them(like'\n') out in the string.
                var cleanedString = Regex.Escape(noteFilter.BodyMatch);
                //Adds a filter to the current filter set. This filter is a contains filter to find if the
                //specified field contains the provided string
                filter &= builder.Regex(data => data.Precipitation, BsonRegularExpression.Create(cleanedString));
            }
            if (noteFilter.CreatedBefore != null)
            {
                //Creates a Less than or equal to filter that checks the created date of the note against the
                //Created before date of the noteFilter
                filter &= builder.Lte(data => data.Time, noteFilter.CreatedBefore.Value);
            }
            if (noteFilter.CreatedAfter != null)
            {
                //Creates a greater than or equal to filter that checks the created date of the note against the
                //Created after date of the noteFilter
                filter &= builder.Gte(data => data.Time, noteFilter.CreatedAfter.Value);
            }

            //Returns the completed filter definitions to the caller.
            return filter;
        }

        private UpdateDefinition<WeatherSensor> GenerateUpdateDefinition(WeatherPatchDetailsDTO details ) 
        {
            //Creates a filter builder to allow us to build update rules in a way that
            ////allows us to append extra rules to them afterwards.
            var builder = Builders<WeatherSensor>.Update.Combine();
            //Declare an update definition object which starts as null.
            UpdateDefinition<WeatherSensor> updateRules = null;
            
            //If the title field of the details is not empty, create a new rule for updating the
            //title property in the notes rules.
            if (string.IsNullOrWhiteSpace(details.deviceName) == false)
            {
                updateRules = builder.Set(data => data.deviceName, details.deviceName);
            }

            //If the title field of the details is not empty, create a new rule for updating the
            //title property in the notes rules.
            if (string.IsNullOrWhiteSpace(details.deviceName) == false)
            {
                //Check if I have any update rules set yet, if not use the builder to create
                //a new definitions object and set its first rule.
                if (updateRules == null)
                {
                    updateRules = builder.Set(data => data.atmosphericPressure, details.Temperature);
                }
                //If there is already at least one exisiting rule, add a new one to the set. 
                else 
                {
                    updateRules = updateRules.Set(data => data.atmosphericPressure, details.Temperature);
                }
            }

            //Returns the completed update definitions/rules to the caller 
            return updateRules;
        }

        
    }
}
