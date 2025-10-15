using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HippoExchange.Api.Models
{
    public enum AssetStatus
    {
        Available,
        In_Repair,
        Unlisted
    }

    public class Assets
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required, StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Item name can only contain letter, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, (-), (&), and (').")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z-]+$",
        ErrorMessage = "Category name can only contain letters, and (-). ")]
        public string Category { get; set; } = string.Empty;

        [CustomValidation(typeof(Assets), nameof(ValidatePastOrPresentDate))]
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

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(AssetStatus))]
        [BsonRepresentation(BsonType.String)]
        public AssetStatus Status { get; set; }

        public bool Favorite { get; set; } = false;

        public static ValidationResult? ValidatePastOrPresentDate(DateTime date, ValidationContext context)
        {
            if (date.ToUniversalTime().Date > DateTime.UtcNow.Date)
                return new ValidationResult("Purchase date cannot be in the future.");
            return ValidationResult.Success;
        }
    }

    public class CreateAssetRequest
    {
        [Required, StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Item name can only contain letter, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, (-), (&), and (').")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z-]+$",
        ErrorMessage = "Category name can only contain letters, and (-). ")]
        public string Category { get; set; } = string.Empty;

        [CustomValidation(typeof(Assets), nameof(Assets.ValidatePastOrPresentDate))]
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

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(AssetStatus))]
        public AssetStatus Status { get; set; }

        public bool Favorite { get; set; } = false;
    }

    public class UpdateAssetRequest
    {
        [StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Item name can only contain letter, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string? ItemName { get; set; }

        [StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, (-), (&), and (').")]
        public string? BrandName { get; set; }

        [StringLength(maximumLength: 100, MinimumLength = 3,
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        [RegularExpression(@"^[a-zA-Z-]+$",
        ErrorMessage = "Category name can only contain letters, and (-). ")]
        public string? Category { get; set; }

        [CustomValidation(typeof(Assets), nameof(Assets.ValidatePastOrPresentDate))]
        public DateTime? PurchaseDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Purchase value can't be negative")]
        public decimal? PurchaseCost { get; set; }

        [StringLength(maximumLength: 100  , MinimumLength = 3,  
        ErrorMessage = "Max length is 100 character and a minimum of 3")]
        public string? CurrentLocation { get; set; }

        public List<string>? Images { get; set; }

        [StringLength(maximumLength: 1000  , MinimumLength = 0,  
        ErrorMessage = "Max length is 1000 character and a minimum of 0")]
        public string? ConditionDescription { get; set; }

        [EnumDataType(typeof(AssetStatus))]
        public AssetStatus? Status { get; set; }

        public bool? Favorite { get; set; }
    }
}