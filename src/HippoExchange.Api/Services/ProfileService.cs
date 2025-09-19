using HippoExchange.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HippoExchange.Services;

public class MongoSettings
{
    public string ConnectionString { get; set; } = default!;
    public string Database { get; set; } = default!;
}

public class ProfileService
{
    private readonly IMongoCollection<PersonalProfile> _profiles;

    public ProfileService(IOptions<MongoSettings> opt)
    {
        var client = new MongoClient(opt.Value.ConnectionString);
        var db = client.GetDatabase(opt.Value.Database);
        _profiles = db.GetCollection<PersonalProfile>("profiles");
    }

    public async Task<PersonalProfile?> GetByUserIdAsync(string userId) =>
        await _profiles.Find(p => p.UserId == userId).FirstOrDefaultAsync();

    public async Task UpsertAsync(PersonalProfile profile)
    {
        await _profiles.ReplaceOneAsync(p => p.UserId == profile.UserId, profile,
            new ReplaceOptions { IsUpsert = true });
    }
}