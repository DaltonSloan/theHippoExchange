using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;

namespace HippoExchange.Models
{
    public class Asset
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }

        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public string CurrentLocation { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public string ConditionDescription { get; set; } = string.Empty;
        public string OwnerUserId { get; set; } = string.Empty;
        public string Status { get; set; } = "available"; // e.g., "available", "loaned", "maintenance"
    }
}
