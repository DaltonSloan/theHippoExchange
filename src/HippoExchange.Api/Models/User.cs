using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HippoExchange.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }

        public string ClerkUserId { get; set; } = default!;
                public string? Username { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }
}
