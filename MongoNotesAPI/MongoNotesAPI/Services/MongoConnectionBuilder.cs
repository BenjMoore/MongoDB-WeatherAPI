using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoNotesAPI.Settings;

namespace MongoNotesAPI.Services
{
    public class MongoConnectionBuilder
    {
        //Variable to hold the settings class once it is recieved
        private readonly IOptions<MongoConnectionSettings> _settings;
        
        //Constructor which requests the MOngoConneciton Settings by declaring it as
        //required within the constructor parameters. These will be provided
        //by the dependency injection system.
        public MongoConnectionBuilder(IOptions<MongoConnectionSettings> settings)
        {
            _settings= settings;
        }

        /// <summary>
        /// Method to create and configure a MongoDB connection
        /// </summary>
        /// <returns>A completed MongoDB connection object</returns>
        public IMongoDatabase GetDatabase() 
        {
            var client = new MongoClient(_settings.Value.ConnectionString);
            return client.GetDatabase(_settings.Value.DatabaseName);
        }

        public IMongoDatabase GetDatabase(string database)
        {
            var client = new MongoClient(_settings.Value.ConnectionString);
            return client.GetDatabase(database);
        }

        public IMongoDatabase GetDatabase(string connString, string database)
        {
            var client = new MongoClient(connString);
            return client.GetDatabase(database);
        }
    }
}
