using HippoExchange.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HippoExchange.Services
{
    public class ProfileService
    {
        private readonly IMongoCollection<PersonalProfile> _profiles;

        public ProfileService(IOptions<MongoSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _profiles = database.GetCollection<PersonalProfile>("profiles");
        }

        public async Task<PersonalProfile?> GetByUserIdAsync(string userId) =>
            await _profiles.Find(p => p.UserId == userId).FirstOrDefaultAsync();

        public async Task UpsertAsync(PersonalProfile profile)
        {
            var options = new ReplaceOptions { IsUpsert = true };
            await _profiles.ReplaceOneAsync(p => p.UserId == profile.UserId, profile, options);
        }

        
    }
}
