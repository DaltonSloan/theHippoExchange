using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using HippoExchange.Models;

namespace HippoExchange.Api.Models
{
    public class Maintenance
    {
        public Maintenance()
        {
            // Constructor for Maintenance class
        }

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
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, (-), (&), and (').")]
        public string BrandName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(maximumLength: 150, MinimumLength = 2,
        ErrorMessage = "Product name must be between 2 and 150 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Product name can only contain letter, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ProductName { get; set; } = string.Empty;

        public string? PurchaseLocation { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$",
        ErrorMessage = "Category name can only contain letters, spaces, and (-).")]
        public string AssetCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost paid is required.")]
        [Range(0.01, 1000000,
        ErrorMessage = "Cost must be between 0.01 and 1,000,000.")]
        public decimal CostPaid { get; set; }

        [Required(ErrorMessage = "Maintenance due date is required.")]
        [CustomValidation(typeof(Maintenance), nameof(ValidateFutureDate))]
        public DateTime MaintenanceDueDate { get; set; }

        //Custom validator
        public static ValidationResult? ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date.Date < DateTime.Today)
                return new ValidationResult("Maintenance due date must be today or in the future.");
            return ValidationResult.Success;
        }

        [Required(ErrorMessage = "Maintenance title is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Maintenance title must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\&',\.]+$",
        ErrorMessage = "Maintenance title can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string MaintenanceTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance description is required.")]
        [StringLength(maximumLength: 1000,
        ErrorMessage = "Maintenance description cannot exceed 1000 characters.")]
        public string MaintenanceDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance status is required.")]
        [RegularExpression("Upcoming|Overdue|Completed", ErrorMessage = "Status must be Upcoming, Overdue, or Completed.")]
        public string MaintenanceStatus { get; set; } = "Upcoming";

        public bool PreserveFromPrior { get; set; }

        public bool IsCompleted { get; set; } = false; // or true if we need to change it.

        [Required(ErrorMessage = "At least one required tool must be specified.")]
        public List<string> RequiredTools { get; set; } = new List<string>();

        [Required(ErrorMessage = "Tool location is required.")]
        [StringLength(maximumLength: 200, MinimumLength = 2,
        ErrorMessage = "Tool location must be between 2 and 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s-\&',.]+$",
        ErrorMessage = "Tool location can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ToolLocation { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public RecurrenceUnit? RecurrenceUnit { get; set; }
        public int? RecurrenceInterval { get; set; }
    }

    public class CreateMaintenanceRequest
    {
        [Required]
        public string AssetId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Max length is 100 character and the minimum is 2.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, (-), (&), and (').")]
        public string BrandName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(maximumLength: 150, MinimumLength = 2,
        ErrorMessage = "Product name must be between 2 and 150 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Product name can only contain letter, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ProductName { get; set; } = string.Empty;

        public string? PurchaseLocation { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$",
        ErrorMessage = "Category name can only contain letters, spaces, and (-).")]
        public string AssetCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost paid is required.")]
        [Range(0.01, 1000000,
        ErrorMessage = "Cost must be between 0.01 and 1,000,000.")]
        public decimal CostPaid { get; set; }

        [Required(ErrorMessage = "Maintenance due date is required.")]
        [CustomValidation(typeof(Maintenance), nameof(ValidateFutureDate))]
        public DateTime MaintenanceDueDate { get; set; }

        //Custom validator
        public static ValidationResult? ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date.Date < DateTime.Today)
                return new ValidationResult("Maintenance due date must be today or in the future.");
            return ValidationResult.Success;
        }

        [Required(ErrorMessage = "Maintenance title is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 2,
        ErrorMessage = "Maintenance title must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\&',\.]+$",
        ErrorMessage = "Maintenance title can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string MaintenanceTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance description is required.")]
        [StringLength(maximumLength: 1000,
        ErrorMessage = "Maintenance description cannot exceed 1000 characters.")]
        public string MaintenanceDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maintenance status is required.")]
        [RegularExpression("Upcoming|Overdue|Completed", ErrorMessage = "Status must be Upcoming, Overdue, or Completed.")]
        public string MaintenanceStatus { get; set; } = "Upcoming";
        
        public bool PreserveFromPrior { get; set; }
        
        public bool IsCompleted { get; set; } = false;

        [Required(ErrorMessage = "At least one required tool must be specified.")]
        [StringLength(maximumLength: 500, MinimumLength = 1,
        ErrorMessage = "Required tools must be between 1 and 500 characters.")]
        public string RequiredTools { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tool location is required.")]
        [StringLength(maximumLength: 200, MinimumLength = 2,
        ErrorMessage = "Tool location must be between 2 and 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s-\&',.]+$",
        ErrorMessage = "Tool location can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ToolLocation { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        public RecurrenceUnit? RecurrenceUnit { get; set; }
        public int? RecurrenceInterval { get; set; }
    }

    public class UpdateMaintenanceRequest
    {
        [StringLength(maximumLength: 100,
        ErrorMessage = "Max length is 100 character and the minimum is 0.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$",
        ErrorMessage = "Brand name can only contain letter, numbers, spaces, (-), (&), and (').")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(maximumLength: 150,
        ErrorMessage = "Product name must be between 0 and 150 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$",
        ErrorMessage = "Product name can only contain letter, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ProductName { get; set; } = string.Empty;

        public string? PurchaseLocation { get; set; }

        [StringLength(maximumLength: 100,
        ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$",
        ErrorMessage = "Category name can only contain letters, spaces, and (-).")]
        public string AssetCategory { get; set; } = string.Empty;

        [Range(0.01, 1000000,
        ErrorMessage = "Cost must be between 0.01 and 1,000,000.")]
        public decimal CostPaid { get; set; }

        [CustomValidation(typeof(Maintenance), nameof(ValidateFutureDate))]
        public DateTime MaintenanceDueDate { get; set; }

        //Custom validator
        public static ValidationResult? ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date.Date < DateTime.Today)
                return new ValidationResult("Maintenance due date must be today or in the future.");
            return ValidationResult.Success;
        }

        [StringLength(maximumLength: 100, MinimumLength = 0,
        ErrorMessage = "Maintenance title must be between 0 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\&',\.]+$",
        ErrorMessage = "Maintenance title can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string MaintenanceTitle { get; set; } = string.Empty;

        [StringLength(maximumLength: 1000,
        ErrorMessage = "Maintenance description cannot exceed 1000 characters.")]
        public string MaintenanceDescription { get; set; } = string.Empty;

        [RegularExpression("Upcoming|Overdue|Completed", ErrorMessage = "Status must be Upcoming, Overdue, or Completed.")]
        public string MaintenanceStatus { get; set; } = "Upcoming";
        
        public bool PreserveFromPrior { get; set; }
        
        public bool IsCompleted { get; set; }

        [StringLength(maximumLength: 500, MinimumLength = 1,
        ErrorMessage = "Required tools must be between 1 and 500 characters.")]
        public string RequiredTools { get; set; } = string.Empty;

        [StringLength(maximumLength: 200,
        ErrorMessage = "Tool location must be between 0 and 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s-\&',.]+$",
        ErrorMessage = "Tool location can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string ToolLocation { get; set; } = string.Empty;
        
        public RecurrenceUnit? RecurrenceUnit { get; set; }
        public int? RecurrenceInterval { get; set; }
    }

    public class PatchMaintenanceRequest
    {
        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "Max length is 100 character and the minimum is 2.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&']+$", ErrorMessage = "Brand name can only contain letter, numbers, spaces, (-), (&), and (').")]
        public string? BrandName { get; set; }

        [StringLength(maximumLength: 150, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 150 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s\-\&',\.]+$", ErrorMessage = "Product name can only contain letter, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string? ProductName { get; set; }

        public string? PurchaseLocation { get; set; }

        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$", ErrorMessage = "Category name can only contain letters, spaces, and (-). ")]
        public string? AssetCategory { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Cost must be between 0.01 and 1,000,000.")]
        public decimal? CostPaid { get; set; }

        [CustomValidation(typeof(Maintenance), nameof(Maintenance.ValidateFutureDate))]
        public DateTime? MaintenanceDueDate { get; set; }

        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "Maintenance title must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\&',\.]+$", ErrorMessage = "Maintenance title can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string? MaintenanceTitle { get; set; }

        [StringLength(maximumLength: 1000, ErrorMessage = "Maintenance description cannot exceed 1000 characters.")]
        public string? MaintenanceDescription { get; set; }

        [RegularExpression("Upcoming|Overdue|Completed", ErrorMessage = "Status must be Upcoming, Overdue, or Completed.")]
        public string? MaintenanceStatus { get; set; }
        
        public bool? PreserveFromPrior { get; set; }
        
        public bool? IsCompleted { get; set; }

        [StringLength(maximumLength: 500, MinimumLength = 1,
        ErrorMessage = "Required tools must be between 1 and 500 characters.")]
        public string? RequiredTools { get; set; }

        [StringLength(maximumLength: 200, MinimumLength = 2, ErrorMessage = "Tool location must be between 2 and 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\s-\&',.]+$", ErrorMessage = "Tool location can only contain letters, numbers, spaces, (-), (,), (.), (&), and (').")]
        public string? ToolLocation { get; set; }
        
        public RecurrenceUnit? RecurrenceUnit { get; set; }
        public int? RecurrenceInterval { get; set; }
    }
}