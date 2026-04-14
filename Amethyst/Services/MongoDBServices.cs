using MongoDB.Driver;

namespace Amethyst.Services
{
    public class MongoDBServices
    {
        private readonly IMongoCollection<Study_Songs> _collection;

        public MongoDBServices()
        {
            MongoClient client = new MongoClient(Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING"));
            IMongoDatabase database = client.GetDatabase(Environment.GetEnvironmentVariable("MONGO_DB_NAME"));
            _collection = database.GetCollection<Study_Songs>(Environment.GetEnvironmentVariable("MONGO_DB_COLLECTION_NAME"));
        }

        public async Task<List<Study_Songs>> GetAllAsync() =>
             await _collection.Find(_ => true).ToListAsync();

        public async Task<Study_Songs> GetAsync(string id) =>
            await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Study_Songs song) =>
            await _collection.InsertOneAsync(song);

        public async Task UpdateAsync(string id, Study_Songs song) =>
            await _collection.ReplaceOneAsync(x => x.Id == id, song);

        public async Task DeleteAsync(string id) =>
            await _collection.DeleteOneAsync(x => x.Id == id);
    }
}
