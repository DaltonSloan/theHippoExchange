using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HippoExchange.Api.Models
{
    public class Assets
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required, StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string Category { get; set; } = string.Empty;

        [Range(typeof(DateTime),"0001-01-01","2025-01-01", 
        ErrorMessage = "Valid dates for the Property {0} between {1} and {2}")]
        public DateTime PurchaseDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase value can't be negative")]
        public decimal PurchaseCost { get; set; }

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string CurrentLocation { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        [StringLength(maximumLength: 1000  , MinimumLength = 0,  
        ErrorMessage = "Max length is 1000 character and a minimum of 0")]
        public string ConditionDescription { get; set; } = string.Empty;

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        [StringLength(maximumLength: 15  , MinimumLength = 3,  
        ErrorMessage = "Max length is 15 character and a minimum of 3")]
        public string Status { get; set; } = string.Empty;

        public bool Favorite { get; set; }
    }

    public class CreateAssetRequest
    {
[Required, StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string Category { get; set; } = string.Empty;

        [Range(typeof(DateTime),"1900-01-01","2025-01-01", 
        ErrorMessage = "Valid dates for the Property {0} between {1} and {2}")]
        public DateTime PurchaseDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase value can't be negative")]
        public decimal PurchaseCost { get; set; }

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string CurrentLocation { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        [StringLength(maximumLength: 1000  , MinimumLength = 0,  
        ErrorMessage = "Max length is 1000 character and a minimum of 0")]
        public string ConditionDescription { get; set; } = string.Empty;

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool Favorite { get; set; }
    }

    public class UpdateAssetRequest
    {
        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string Category { get; set; } = string.Empty;

        [Range(typeof(DateTime),"1900-01-01","2025-01-01", 
        ErrorMessage = "Valid dates for the Property {0} between {1} and {2}")]
        public DateTime PurchaseDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase value can't be negative")]
        public decimal PurchaseCost { get; set; }

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string CurrentLocation { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        [StringLength(maximumLength: 1000  , MinimumLength = 0,  
        ErrorMessage = "Max length is 1000 character and a minimum of 0")]
        public string ConditionDescription { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool Favorite { get; set; }
    }
}

