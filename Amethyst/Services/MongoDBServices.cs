using MongoDB.Driver;

namespace Amethyst.Services
{
    public class MongoDBServices
    {
        private readonly IMongoCollection<Study_Songs> _collection;

        public MongoDBServices(IConfiguration configuration)
        {
            MongoClient client = new MongoClient(configuration["MongoDBSettings:ConnectionString"]);
            IMongoDatabase database = client.GetDatabase(configuration["MongoDBSettings:DatabaseName"]);
            _collection = database.GetCollection<Study_Songs>(configuration["MongoDBSettings:CollectionName"]);
        }

        public async Task<List<Study_Songs>> GetAllSongsAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
    }
}
