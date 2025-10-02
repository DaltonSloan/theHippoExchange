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

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;
        
        public string PerformedBy { get; set; } = string.Empty;

        public decimal Cost { get; set; }
        
        public string Status { get; set; } = "Pending";
    }

    public class CreateMaintenanceRequest
    {
        [Required]
        public string AssetId { get; set; } = string.Empty;
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class UpdateMaintenanceRequest
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Status { get; set; } = "Pending";
    }
}