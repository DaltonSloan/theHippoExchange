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

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public decimal PurchaseCost { get; set; }

        public string CurrentLocation { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        public string ConditionDescription { get; set; } = string.Empty;

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool Favorite { get; set; }
    }

    public class CreateAssetRequest
    {
        [Required]
        public string ItemName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public decimal PurchaseCost { get; set; }

        public string CurrentLocation { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        public string ConditionDescription { get; set; } = string.Empty;

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool Favorite { get; set; }
    }

    public class UpdateAssetRequest
    {
        public string ItemName { get; set; } = string.Empty;

        public string BrandName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public decimal PurchaseCost { get; set; }

        public string CurrentLocation { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new List<string>();

        public string ConditionDescription { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool Favorite { get; set; }
    }
}

