using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HippoExchange.Models;

namespace HippoExchange.Api.Models
{
    public class Maintenance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AssetId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Max length is 100 character and the minimum is 2.")]
        //Regular Expression means that these characters given are characters allowed to by entered into that field
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, hyphens, ampersands, and apostrophes.")]
        public string BrandName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(maximumLength: 150, MinimumLength = 2,
        ErrorMessage = "Product name must be between 2 and 150 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Product name can only contain letter, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost paid is required.")]
        [Range(0.01, 1000000,
        ErrorMessage = "Cost must be between 0.01 and 1,000,000.")]
        //Supposed to hint to UI that this variable is currency and should format it as currency. If this breaks frontend delete the line below.
        [DataType(DataType.Currency)]
        public decimal CostPaid { get; set; }

        [Required(ErrorMessage = "Maintenance due date is required.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Maintenance), nameof(ValidateFutureDate))]
        public DateTime MaintenanceDueDate { get; set; }

        //Custom validator
        public static ValidationResult? ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date < DateTime.Today)
                return new ValidationResult("Maintenance due date must be today or in the future.");
            return ValidationResult.Success;
        }

        [Required(ErrorMessage = "Maintenance title is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Maintenance title must be between 22 and 100 characters.")]
        [RegularExpression(@"^a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Maintenance title can only contain letters, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string MaintenanceTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance description is required.")]
        [StringLength(maximumLength: 1000,
        ErrorMessage = "Maintenance description cannot exceed 1000 characters.")]
        public string MaintenanceDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance status is required.")]
        [RegularExpression("Upcoming|Overdue|Completed", ErrorMessage = "Status must be Upcoming, Overdue, or Completed.")]
        public string MaintenanceStatus { get; set; } = "Upcoming";

        public bool PreserveFromPrior { get; set; } = false; // or true if we need to change it.

        [Required(ErrorMessage = "At least one required tool must be specified.")]
        [MinLength(1, ErrorMessage = "At least one required tool must be specified.")]
        public List<string> RequiredTools { get; set; } = new List<string>();

        [Required(ErrorMessage = "Tool location is required.")]
        [StringLength(maximumLength: 200, MinimumLength = 2,
        ErrorMessage = "Tool location must be between 2 and 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Tool location can only contain letters, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string ToolLocation { get; set; } = string.Empty;
    }

    public class CreateMaintenanceRequest
    {
        [Required]
        public string AssetId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Max length is 100 character and the minimum is 2.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, hyphens, ampersands, and apostrophes.")]
        public string BrandName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(maximumLength: 150, MinimumLength = 2,
        ErrorMessage = "Product name must be between 2 and 150 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Product name can only contain letter, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost paid is required.")]
        [Range(0.01, 1000000,
        ErrorMessage = "Cost must be between 0.01 and 1,000,000.")]
        //Supposed to hint to UI that this variable is currency and should format it as currency. If this breaks frontend delete the line below.
        [DataType(DataType.Currency)]
        public decimal CostPaid { get; set; }

        [Required(ErrorMessage = "Maintenance due date is required.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Maintenance), nameof(ValidateFutureDate))]
        public DateTime MaintenanceDueDate { get; set; }

        //Custom validator
        public static ValidationResult? ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date < DateTime.Today)
                return new ValidationResult("Maintenance due date must be today or in the future.");
            return ValidationResult.Success;
        }

        [Required(ErrorMessage = "Maintenance title is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Maintenance title must be between 22 and 100 characters.")]
        [RegularExpression(@"^a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Maintenance title can only contain letters, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string MaintenanceTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance description is required.")]
        [StringLength(maximumLength: 1000,
        ErrorMessage = "Maintenance description cannot exceed 1000 characters.")]
        public string MaintenanceDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance status is required.")]
        [RegularExpression("Upcoming|Overdue|Completed", ErrorMessage = "Status must be Upcoming, Overdue, or Completed.")]
        public string MaintenanceStatus { get; set; } = "Upcoming";
        public bool PreserveFromPrior { get; set; } = false;

        [Required(ErrorMessage = "At least one required tool must be specified.")]
        [MinLength(1, ErrorMessage = "At least one required tool must be specified.")]
        public List<string> RequiredTools { get; set; } = new List<string>();

        [Required(ErrorMessage = "Tool location is required.")]
        [StringLength(maximumLength: 200, MinimumLength = 2,
        ErrorMessage = "Tool location must be between 2 and 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Tool location can only contain letters, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string ToolLocation { get; set; } = string.Empty;
    }

    public class UpdateMaintenanceRequest
    {
        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Max length is 100 character and the minimum is 2.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, hyphens, ampersands, and apostrophes.")]
        public string BrandName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(maximumLength: 150, MinimumLength = 2,
        ErrorMessage = "Product name must be between 2 and 150 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Product name can only contain letter, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost paid is required.")]
        [Range(0.01, 1000000,
        ErrorMessage = "Cost must be between 0.01 and 1,000,000.")]
        //Supposed to hint to UI that this variable is currency and should format it as currency. If this breaks frontend delete the line below.
        [DataType(DataType.Currency)]
        public decimal CostPaid { get; set; }

        [Required(ErrorMessage = "Maintenance due date is required.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Maintenance), nameof(ValidateFutureDate))]
        public DateTime MaintenanceDueDate { get; set; }

        //Custom validator
        public static ValidationResult? ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date < DateTime.Today)
                return new ValidationResult("Maintenance due date must be today or in the future.");
            return ValidationResult.Success;
        }

        [Required(ErrorMessage = "Maintenance title is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Maintenance title must be between 22 and 100 characters.")]
        [RegularExpression(@"^a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Maintenance title can only contain letters, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string MaintenanceTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance description is required.")]
        [StringLength(maximumLength: 1000,
        ErrorMessage = "Maintenance description cannot exceed 1000 characters.")]
        public string MaintenanceDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance status is required.")]
        [RegularExpression("Upcoming|Overdue|Completed", ErrorMessage = "Status must be Upcoming, Overdue, or Completed.")]
        public string MaintenanceStatus { get; set; } = "Upcoming";
        public bool PreserveFromPrior { get; set; } = false;

        [Required(ErrorMessage = "At least one required tool must be specified.")]
        [MinLength(1, ErrorMessage = "At least one required tool must be specified.")]
        public List<string> RequiredTools { get; set; } = new List<string>();

        [Required(ErrorMessage = "Tool location is required.")]
        [StringLength(maximumLength: 200, MinimumLength = 2,
        ErrorMessage = "Tool location must be between 2 and 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Tool location can only contain letters, numbers, spaces, hyphens, commas, periods, ampersands, and apostrophes.")]
        public string ToolLocation { get; set; } = string.Empty;
    }
}