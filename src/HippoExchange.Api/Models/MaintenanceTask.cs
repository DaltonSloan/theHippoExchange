using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace HippoExchange.Models
{
    public class MaintenanceTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string AssetId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Frequency { get; set; } = "Once"; // e.g., "Once", "Daily", "Weekly", "Monthly", "Yearly"
        public bool PreserveOnComplete { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
    }
}
