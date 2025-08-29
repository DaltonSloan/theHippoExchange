using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HypoExchange.Models;

public class PersonalProfile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string UserId { get; set; } = default!;
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
}
