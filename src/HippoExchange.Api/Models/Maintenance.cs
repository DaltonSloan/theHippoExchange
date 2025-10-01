using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;

namespace HippoExchange.Models
{
    public class Maintenance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AssetId { get; set; } = string.Empty;

        // Basic product info
        public string BrandName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        // Purchase info
        // Purchase Location example: "Lowes", "Home Depot"
        public string PurchaseLocation { get; set; } = string.Empty;
        public decimal CostPaid { get; set; }

        // Maintenace details
        public DateTime MaintenanceDueDate { get; set; }
        public string MaintenanceTitle { get; set; } = string.Empty;
        public string MaintenanceDescription { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = "pending"; // or could be "completed"

        // History and preservation
        public bool PreserveFromPrior { get; set; }

        // Attributes required to know about the tools
        public string RequiredTools { get; set; } = string.Empty;
        public string ToolLocation { get; set; } = string.Empty;
    }
}