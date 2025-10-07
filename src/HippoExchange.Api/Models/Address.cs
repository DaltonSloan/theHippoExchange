using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace HippoExchange.Models
{
    public class Address
    {
        [JsonPropertyName("street")]
        [StringLength(maximumLength: 100  , MinimumLength = 0,  
        ErrorMessage = "Max length is 100 character and a minimum of 0")]
        public string? Street { get; set; }

        [JsonPropertyName("city")]
        [StringLength(maximumLength: 100  , MinimumLength = 0,  
        ErrorMessage = "Max length is 100 character and a minimum of 0")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        [StringLength(maximumLength: 100  , MinimumLength = 0,  
        ErrorMessage = "Max length is 100 character and a minimum of 0")]
        public string? State { get; set; }

        [JsonPropertyName("postal_code")]
        [StringLength(maximumLength: 100  , MinimumLength = 0,  
        ErrorMessage = "Max length is 100 character and a minimum of 0")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("country")]
        [StringLength(maximumLength: 100  , MinimumLength = 0,  
        ErrorMessage = "Max length is 100 character and a minimum of 0")]
        public string? Country { get; set; }
    }
}
