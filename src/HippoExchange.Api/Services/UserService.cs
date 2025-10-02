using HippoExchange.Models;
using HippoExchange.Models.Clerk;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq;

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
            // Resolve primary email: prefer PrimaryEmailAddressId match; fallback to first available
            string? primaryEmail = null;
            if (clerkUser.EmailAddresses != null && clerkUser.EmailAddresses.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(clerkUser.PrimaryEmailAddressId))
                {
                    primaryEmail = clerkUser.EmailAddresses
                        .FirstOrDefault(e => e.Id == clerkUser.PrimaryEmailAddressId)?.EmailAddress;
                }
                primaryEmail ??= clerkUser.EmailAddresses.FirstOrDefault()?.EmailAddress;
            }

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
                UpdatedAt = clerkUser.UpdatedAt,
                EmailAddresses = clerkUser.EmailAddresses,
                Email = primaryEmail ?? string.Empty,
                ContactInformation = new ContactInformation
                {
                    Email = primaryEmail ?? string.Empty,
                }
            };

            var filter = Builders<User>.Filter.Eq(u => u.ClerkId, user.ClerkId);
            var options = new ReplaceOptions { IsUpsert = true };
            await _usersCollection.ReplaceOneAsync(filter, user, options);
        }

        public async Task<List<User>> GetAllUsersAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User> GetByClerkIdAsync(string clerkId) =>
            await _usersCollection.Find(u => u.ClerkId == clerkId).FirstOrDefaultAsync();

        public async Task<bool> UpdateUserProfileAsync(string clerkId, ProfileUpdateRequest updateRequest)
        {
            var filter = Builders<User>.Filter.Eq(u => u.ClerkId, clerkId);
            var update = Builders<User>.Update
                .Set(u => u.PhoneNumber, updateRequest.PhoneNumber)
                .Set(u => u.Address, updateRequest.Address)
                .Set(u => u.FirstName, updateRequest.FirstName)
                .Set(u => u.LastName, updateRequest.LastName);

            var result = await _usersCollection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged;
        }

        public async Task DeleteUserAsync(string clerkId) =>
            await _usersCollection.DeleteOneAsync(u => u.ClerkId == clerkId);


/*
        public async Task CreateNewAssets(string userId, Assets newAsset)
        {
            //Filters through useres until we find the user we want to inset a new asset for
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            //push the user newasset into the asset list
            var update = Builders<User>.Update.Push(u => u.Assets, newAsset);
            //retrives meta data to use else where
            var result = await _usersCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)//error handeling if account is not found
            {
                throw new Exception($"User with ClerkId {userId} not found.");
            }

        }
*/
    }
}