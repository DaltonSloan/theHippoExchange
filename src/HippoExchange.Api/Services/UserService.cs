using HippoExchange.Models;
using HippoExchange.Models.Clerk;
using HippoExchange.Api.Models;
using MongoDB.Driver;

namespace HippoExchange.Api.Services
{
    /// <summary>
    /// Handles CRUD operations for application users synchronised from Clerk.
    /// </summary>
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        /// <summary>
        /// Creates a new <see cref="UserService"/> instance using the shared database connection.
        /// </summary>
        public UserService(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("users");
        }

        /// <summary>
        /// Inserts a new user document.
        /// </summary>
        public async Task CreateUserAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        /// <summary>
        /// Retrieves a user by Clerk identifier.
        /// </summary>
        public async Task<User?> GetUserByClerkIdAsync(string clerkId) =>
            await _usersCollection.Find(u => u.ClerkId == clerkId).FirstOrDefaultAsync();

        /// <summary>
        /// Upserts (create or replace) a user document matching the incoming Clerk payload.
        /// </summary>
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
                UpdatedAt = clerkUser.UpdatedAt,
                EmailAddresses = clerkUser.EmailAddresses
            };

            var filter = Builders<User>.Filter.Eq(u => u.ClerkId, user.ClerkId);
            var options = new ReplaceOptions { IsUpsert = true };
            await _usersCollection.ReplaceOneAsync(filter, user, options);
        }

        /// <summary>
        /// Returns all user documents.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        /// <summary>
        /// Retrieves a user document by Clerk identifier.
        /// </summary>
        public async Task<User> GetByClerkIdAsync(string clerkId) =>
            await _usersCollection.Find(u => u.ClerkId == clerkId).FirstOrDefaultAsync();

        /// <summary>
        /// Updates a user's profile data from the application-provided payload.
        /// </summary>
        public async Task<bool> UpdateUserProfileAsync(string clerkId, ProfileUpdateRequest updateRequest)
        {
            var filter = Builders<User>.Filter.Eq(u => u.ClerkId, clerkId);
            var update = Builders<User>.Update
                .Set(u => u.PhoneNumber, updateRequest.PhoneNumber)
                .Set(u => u.Address, updateRequest.Address);

            var result = await _usersCollection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        /// <summary>
        /// Deletes a user and cascades dependent data handled elsewhere.
        /// </summary>
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
