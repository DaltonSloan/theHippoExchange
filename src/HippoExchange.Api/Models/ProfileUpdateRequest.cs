using System.Text.Json.Serialization;

namespace HippoExchange.Models
{
    public class ProfileUpdateRequest
    {
        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }
        
        [JsonPropertyName("address")]
        public Address? Address { get; set; }
    }
}
