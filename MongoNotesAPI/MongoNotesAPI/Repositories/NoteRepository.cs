using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoNotesAPI.Models;
using MongoNotesAPI.Models.Filters;
using MongoNotesAPI.Services;
using System.Text.RegularExpressions;

namespace MongoNotesAPI.Repositories
{
    public class NoteRepository : INoteRepository
    {
        //Creating a readonly variable to hold our note collection details 
        private readonly IMongoCollection<Note> _notes;
        
        //Ask for the MongoConnectionBuilder from the dependency injection by requesting
        //it in the parameters of the constructor.
        public NoteRepository(MongoConnectionBuilder connection)
        {
            //Use the connection to access the database and find the collection
            //called notes. The passing of the Note model into this method indicates that
            //the documents of the collection should be mapped to note objects.
            _notes = connection.GetDatabase().GetCollection<Note>("Notes");
        }


        public void Create(Note createdNote)
        {
            _notes.InsertOne(createdNote);
        }

        public void CreateMany(List<Note> noteList)
        {
            _notes.InsertMany(noteList);
        }

        public OperationResponseDTO<Note> Delete(string id)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<Note>.Filter.Eq(note => note._id, objId);
            //Use the filter to find the required record and delete it.
            var result = _notes.DeleteOne(filter);

            //Check if any records were deleted by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.DeletedCount > 0)
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "Note Deleted Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "No notes deleted. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public OperationResponseDTO<Note> DeleteMany(NoteFilter noteFilter)
        {
            //Passes the provided filter to the method that will build a set of mongo db
            //filter definitions.
            var filter = GenerateFilterDefinition(noteFilter);
            //Sends the delete request to MongoDB to delete all entries matching the filter
            var result = _notes.DeleteMany(filter);

            //Check if any records were deleted by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.DeletedCount > 0)
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "Note/s Deleted Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "No notes deleted. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public IEnumerable<Note> GetAll()
        {
            //Requests a filter builder for the Note model from the builders class
            var builder = Builders<Note>.Filter;
            //Uses the filter builder to create an empty filter(no filter options)
            var filter = builder.Empty;
            //Sends the find request to MongoDB to return all entries matching the filter
            //which in this case will be every entry and then put them in a collection
            return _notes.Find(filter).ToEnumerable();
        }

        public IEnumerable<Note> GetAll(NoteFilter noteFilter)
        {
            //Passes the provided filter to the method that will build a set of mongo db
            //filter definitions.
            var filter = GenerateFilterDefinition(noteFilter);

            //Sends the find request to MongoDB to return all entries matching the filter
            //which in this case will be every entry and then put them in a collection
            return _notes.Find(filter).ToEnumerable();
        }

        public Note GetById(string id)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<Note>.Filter.Eq(note => note._id, objId);
            //Call the find method using the filter to find the first recod that matches.
            return _notes.Find(filter).FirstOrDefault();
        }

        public OperationResponseDTO<Note> Update(string id, Note updatedNote)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<Note>.Filter.Eq(note => note._id, objId);

            //Create an update builder to allow the creation of our set of update
            //definitions.
            var builder = Builders<Note>.Update;
            //Define a set of update definitions(rules) to outline what fields need
            //to be updated and what they need to be updated to.
            var update = builder.Set(note => note.Title, updatedNote.Title)
                                .Set(note => note.Body, updatedNote.Body);

            //Call the update method and give it the filter to find the desired entry as 
            //well as the update definitions of what fields need to be changed.
            var result = _notes.UpdateOne(filter, update);

            //Check if any records were chnaged by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.ModifiedCount > 0)
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "Note Updated Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "No notes updated. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        public OperationResponseDTO<Note> UpdateMany(NotePatchDetailsDTO details)
        {
            //Passes the provided filter parameters to the method to build the filter rules
            //for mongo db.
            var filterDefinitions = GenerateFilterDefinition(details.Filter);
            //Passes the provided details parameters to the method to build the update rules
            //for mongo db.
            var updateDefinitions = GenerateUpdateDefinition(details);
            //Pass the filter and update rules to mongo db to process our request.
            var result = _notes.UpdateMany(filterDefinitions, updateDefinitions);

            //Check if any records were chnaged by mongo db and send back details
            //regarding the success/failure of thr changes
            if (result.ModifiedCount > 0)
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "Note/s Updated Successfully",
                    WasSuccessful = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResponseDTO<Note>
                {
                    Message = "No notes updated. Please check details and try again.",
                    WasSuccessful = false,
                    RecordsAffected = 0
                };
            }
        }

        //This is an alternate variation of the above upsate mthod which does a full
        //find and replace of the note object.
        public void Replace(string id, Note updatedNote)
        {
            //Takes the id string and converts it back to an Object Id in the format 
            //requiired by MongoDB
            ObjectId objId = ObjectId.Parse(id);
            //Uses the filter builder to create a single filter that looks for an equals
            //match on the note's id against the provided objId.
            var filter = Builders<Note>.Filter.Eq(note => note._id, objId);
            //Pass the converted object Id into the updated note
            updatedNote._id = objId;
            //Call the replace method and give it the filter to find the desired entry as 
            //well as the updated note details that it will be replaced with.
            _notes.ReplaceOne(filter, updatedNote);
        }

        private FilterDefinition<Note> GenerateFilterDefinition(NoteFilter noteFilter)
        {
            //Requests a filter builder for the Note model from the builders class
            var builder = Builders<Note>.Filter;
            //Uses the filter builder to create an empty filter(no filter options)
            var filter = builder.Empty;

            if (String.IsNullOrEmpty(noteFilter.TitleMatch) == false)
            {
                //Cleans the original string to remove any charactes that might cause issues with our
                //regex filter by escaping them(like'\n') out in the string.
                var cleanedString = Regex.Escape(noteFilter.TitleMatch);
                //Adds a filter to the current filter set. This filter is a contains filter to find if the
                //specified field contains the provided string
                filter &= builder.Regex(note => note.Title, BsonRegularExpression.Create(cleanedString));
            }
            if (String.IsNullOrEmpty(noteFilter.BodyMatch) == false)
            {
                //Cleans the original string to remove any charactes that might cause issues with our
                //regex filter by escaping them(like'\n') out in the string.
                var cleanedString = Regex.Escape(noteFilter.BodyMatch);
                //Adds a filter to the current filter set. This filter is a contains filter to find if the
                //specified field contains the provided string
                filter &= builder.Regex(note => note.Body, BsonRegularExpression.Create(cleanedString));
            }
            if (noteFilter.CreatedBefore != null)
            {
                //Creates a Less than or equal to filter that checks the created date of the note against the
                //Created before date of the noteFilter
                filter &= builder.Lte(note => note.Created, noteFilter.CreatedBefore.Value);
            }
            if (noteFilter.CreatedAfter != null)
            {
                //Creates a greater than or equal to filter that checks the created date of the note against the
                //Created after date of the noteFilter
                filter &= builder.Gte(note => note.Created, noteFilter.CreatedAfter.Value);
            }

            //Returns the completed filter definitions to the caller.
            return filter;
        }

        private UpdateDefinition<Note> GenerateUpdateDefinition(NotePatchDetailsDTO details ) 
        {
            //Creates a filter builder to allow us to build update rules in a way that
            ////allows us to append extra rules to them afterwards.
            var builder = Builders<Note>.Update.Combine();
            //Declare an update definition object which starts as null.
            UpdateDefinition<Note> updateRules = null;
            
            //If the title field of the details is not empty, create a new rule for updating the
            //title property in the notes rules.
            if (string.IsNullOrWhiteSpace(details.Title) == false)
            {
                updateRules = builder.Set(note => note.Title, details.Title);
            }

            //If the title field of the details is not empty, create a new rule for updating the
            //title property in the notes rules.
            if (string.IsNullOrWhiteSpace(details.Body) == false)
            {
                //Check if I have any update rules set yet, if not use the builder to create
                //a new definitions object and set its first rule.
                if (updateRules == null)
                {
                    updateRules = builder.Set(note => note.Body, details.Body);
                }
                //If there is already at least one exisiting rule, add a new one to the set. 
                else 
                {
                    updateRules = updateRules.Set(note => note.Body, details.Body);
                }
            }

            //Returns the completed update definitions/rules to the caller 
            return updateRules;
        }

        
    }
}
