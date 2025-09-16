using System.Text.Json.Serialization;

<<<<<<< HEAD:src/HypoExchange.Api/Models/ClerkModels.cs
namespace HypoExchange.Models.Clerk;
=======
namespace HippoExchange.Models.Clerk;
>>>>>>> dev:src/HippoExchange.Api/Models/ClerkModels.cs

public class ClerkWebhookPayload
{
    [JsonPropertyName("data")]
    public ClerkUserData Data { get; set; } = new();

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class ClerkUserData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("profile_image_url")]
    public string ProfileImageUrl { get; set; } = string.Empty;
}
