using MongoDB.Driver;

namespace MongoClient
{
    public class Client
    {
        private readonly string _connectionString;
        private readonly IMongoClient _client;

        public Client(string connectionString)
        {
            _connectionString = connectionString;
            _client = new MongoDB.Driver.MongoClient(connectionString);
        }

        public IMongoCollection<T> GetCollection<T>(string databaseName, string collectionName)
        {
            return _client.GetDatabase(databaseName).GetCollection<T>(collectionName);
        }

        public IMongoDatabase GetDatabase(string databaseName)
        {
            return _client.GetDatabase(databaseName);
        }
    }
}
