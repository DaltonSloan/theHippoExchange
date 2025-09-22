using HippoExchange.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HippoExchange.Services
{
    public class EmailService
    {
        private readonly IMongoCollection<Email> _emails;

        public EmailService(IOptions<MongoSettings> opt)
        {
            var client = new MongoClient(opt.Value.ConnectionString);
            var database = client.GetDatabase(opt.Value.DatabaseName);
            _emails = database.GetCollection<Email>("emails");
        }

        public async Task CreateEmailAsync(Email email)
        {
            await _emails.InsertOneAsync(email);
        }

        public async Task UpsertEmailAsync(Email email)
        {
            var filter = Builders<Email>.Filter.Eq(e => e.ClerkEmailId, email.ClerkEmailId);
            var options = new ReplaceOptions { IsUpsert = true };
            await _emails.ReplaceOneAsync(filter, email, options);
        }
    }
}