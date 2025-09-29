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

        public async Task UpdateEmailAddress(string clerkEmailId, string newAddress)//This method is to update emails
        {
            //filters the content accessed to only edit the one with given clerkemailId
            var filter = Builders<Email>.Filter.Eq(e => e.ClerkEmailId, clerkEmailId);
            //sets the new email into the file
            var update = Builders<Email>.Update.Set(e => e.EmailAddress, newAddress);
            //finds the email based on the filter and updates it and gets meta data for error handeling 
            var result = await _emails.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)//if a document wasn't found with the email then throw the error
            {
                throw new Exception($"Email with ClerkEmailId {clerkEmailId} not found.");
            }
        }
    }
}