using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace HippoExchange.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("clerk_id")]
        public string ClerkId { get; set; } = default!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string? ProfileImageUrl { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("contact_information")]
        public ContactInformation ContactInformation { get; set; } = new();

        [JsonPropertyName("account_status")]
        public AccountStatus AccountStatus { get; set; } = new();

        [JsonPropertyName("statistics")]
        public Statistics Statistics { get; set; } = new();
    }

    public class ContactInformation
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("preferred_contact_method")]
        public string PreferredContactMethod { get; set; } = "email";
    }

    public class AccountStatus
    {
        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("account_active")]
        public bool AccountActive { get; set; } = true;

        [JsonPropertyName("banned")]
        public bool Banned { get; set; }

        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
    }

    public class Statistics
    {
        [JsonPropertyName("total_assets")]
        public int TotalAssets { get; set; } = 0;

        [JsonPropertyName("assets_loaned")]
        public int AssetsLoaned { get; set; } = 0;

        [JsonPropertyName("assets_borrowed")]
        public int AssetsBorrowed { get; set; } = 0;
    }
}