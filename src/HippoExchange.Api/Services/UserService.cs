using HippoExchange.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HippoExchange.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService(IOptions<MongoSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>("users");
        }

        public async Task CreateUserAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        public async Task<User?> GetUserByClerkIdAsync(string clerkId) =>
            await _usersCollection.Find(u => u.ClerkId == clerkId).FirstOrDefaultAsync();

        public async Task UpsertUserAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.ClerkId, user.ClerkId);
            var options = new ReplaceOptions { IsUpsert = true };
            await _usersCollection.ReplaceOneAsync(filter, user, options);
        }

        public async Task CreateAssets(string userId, Assets newAsset)
        {
            //Filters through useres until we find the user we want to inset a new asset for
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);

            var update = Builders<User>.Update.Push(u => u.Assets, newAsset);

            var result = await _usersCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                throw new Exception($"User with ClerkId {userId} not found.");
            }
           
        }
    }
}