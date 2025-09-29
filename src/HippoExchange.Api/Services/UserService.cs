using HippoExchange.Models;
using HippoExchange.Models.Clerk;
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

        public async Task UpsertUserAsync(ClerkUserData clerkUser)
        {
            var user = new User
            {
                ClerkId = clerkUser.Id,
                Username = clerkUser.Username,
                FirstName = clerkUser.FirstName,
                LastName = clerkUser.LastName,
                ImageUrl = clerkUser.ImageUrl,
                HasImage = clerkUser.HasImage,
                PrimaryEmailAddressId = clerkUser.PrimaryEmailAddressId,
                LastSignInAt = clerkUser.LastSignInAt,
                UpdatedAt = clerkUser.UpdatedAt
            };

            var filter = Builders<User>.Filter.Eq(u => u.ClerkId, user.ClerkId);
            var options = new ReplaceOptions { IsUpsert = true };
            await _usersCollection.ReplaceOneAsync(filter, user, options);
        }

        public async Task<List<User>> GetAllUsersAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User> GetByClerkIdAsync(string clerkId) =>
            await _usersCollection.Find(u => u.ClerkId == clerkId).FirstOrDefaultAsync();

        public async Task DeleteUserAsync(string clerkId) =>
            await _usersCollection.DeleteOneAsync(u => u.ClerkId == clerkId);
    }
}