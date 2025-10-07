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

        public string BrandName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string PurchaseLocation { get; set; } = string.Empty;
        public decimal CostPaid { get; set; }
        public DateTime MaintenanceDueDate { get; set; }
        public string MaintenanceTitle { get; set; } = string.Empty;
        public string MaintenanceDescription { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = "Pending";
        public bool PreserveFromPrior { get; set; }
        public List<string> RequiredTools { get; set; } = new List<string>();
        public string ToolLocation { get; set; } = string.Empty;
    }

    public class CreateMaintenanceRequest
    {
        [Required]
        public string AssetId { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string PurchaseLocation { get; set; } = string.Empty;
        public decimal CostPaid { get; set; }
        public DateTime MaintenanceDueDate { get; set; }
        public string MaintenanceTitle { get; set; } = string.Empty;
        public string MaintenanceDescription { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = "Pending";
        public bool PreserveFromPrior { get; set; }
        public List<string> RequiredTools { get; set; } = new List<string>();
        public string ToolLocation { get; set; } = string.Empty;
    }

    public class UpdateMaintenanceRequest
    {
        public string BrandName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string PurchaseLocation { get; set; } = string.Empty;
        public decimal CostPaid { get; set; }
        public DateTime MaintenanceDueDate { get; set; }
        public string MaintenanceTitle { get; set; } = string.Empty;
        public string MaintenanceDescription { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = "Pending";
        public bool PreserveFromPrior { get; set; }
        public List<string> RequiredTools { get; set; } = new List<string>();
        public string ToolLocation { get; set; } = string.Empty;
    }
}