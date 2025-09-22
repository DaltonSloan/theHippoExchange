using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HippoExchange.Models
{
    public class Email
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }

        public string ClerkUserId { get; set; } = default!;
        public string ClerkEmailId { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public bool Reserved { get; set; }
    }
}
