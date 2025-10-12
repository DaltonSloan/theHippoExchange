using HippoExchange.Models;
using HippoExchange.Models.Clerk;
using HippoExchange.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HippoExchange.Api.Services
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
            // Parse unsafe_metadata for phoneNumber and address
            string? phoneNumber = null;
            Address? address = null;

            if (clerkUser.UnsafeMetadata.HasValue && clerkUser.UnsafeMetadata.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                try
                {
                    // Extract phoneNumber from unsafe_metadata
                    if (clerkUser.UnsafeMetadata.Value.TryGetProperty("phoneNumber", out var phoneElement))
                    {
                        phoneNumber = phoneElement.GetString();
                    }

                    // Extract address from unsafe_metadata
                    if (clerkUser.UnsafeMetadata.Value.TryGetProperty("address", out var addressElement))
                    {
                        address = new Address
                        {
                            Street = addressElement.TryGetProperty("street", out var street) ? street.GetString() : null,
                            City = addressElement.TryGetProperty("city", out var city) ? city.GetString() : null,
                            State = addressElement.TryGetProperty("state", out var state) ? state.GetString() : null,
                            PostalCode = addressElement.TryGetProperty("postal_code", out var postalCode) ? postalCode.GetString() : null,
                            Country = addressElement.TryGetProperty("country", out var country) ? country.GetString() : null
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue with user creation without the unsafe_metadata
                    Console.WriteLine($"Error parsing unsafe_metadata: {ex.Message}");
                }
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
                PhoneNumber = phoneNumber,
                Address = address
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
            
            // Build update definition dynamically based on what's provided
            var updateBuilder = Builders<User>.Update;
            var updates = new List<UpdateDefinition<User>>();

            // Update phone number if provided
            if (updateRequest.PhoneNumber != null)
            {
                updates.Add(updateBuilder.Set(u => u.PhoneNumber, updateRequest.PhoneNumber));
            }

            // Update address if provided (handles nested object)
            if (updateRequest.Address != null)
            {
                updates.Add(updateBuilder.Set(u => u.Address, updateRequest.Address));
            }

            // If no updates were provided, return false
            if (updates.Count == 0)
            {
                return false;
            }

            // Combine all updates
            var combinedUpdate = updateBuilder.Combine(updates);
            
            var result = await _usersCollection.UpdateOneAsync(filter, combinedUpdate);
            return result.IsAcknowledged && result.ModifiedCount > 0;
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