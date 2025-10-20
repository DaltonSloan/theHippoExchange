using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HippoExchange.Api.Models;

public enum BorrowRequestStatus
{
    Pending,
    Approved,
    Denied,
    Returned,
    Cancelled
}

public class BorrowRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [Required]
    public string AssetId { get; set; } = default!;

    [Required]
    public string OwnerUserId { get; set; } = default!;

    [Required]
    public string BorrowerUserId { get; set; } = default!;

    [BsonRepresentation(BsonType.String)]
    public BorrowRequestStatus Status { get; set; } = BorrowRequestStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RequestedFrom { get; set; }

    public DateTime? RequestedUntil { get; set; }

    public string? Message { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public string? OwnerNote { get; set; }
}

public class CreateBorrowRequest
{
    [Required]
    public string AssetId { get; set; } = default!;

    [StringLength(500)]
    public string? Message { get; set; }

    public DateTime? RequestedFrom { get; set; }

    public DateTime? RequestedUntil { get; set; }
}

public class BorrowDecisionRequest
{
    [Required]
    public bool Approve { get; set; }

    public string? Note { get; set; }

    public DateTime? DueAt { get; set; }
}

public class CompleteBorrowRequest
{
    public string? Note { get; set; }
}

public class BorrowRequestSummary
{
    public BorrowRequest Request { get; set; } = default!;
    public BorrowAssetSummary Asset { get; set; } = default!;
    public BorrowUserSummary Counterparty { get; set; } = default!;
}

public class BorrowAssetSummary
{
    public string Id { get; set; } = default!;
    public string ItemName { get; set; } = default!;
    public string? BrandName { get; set; }
    public string? Category { get; set; }
    public string? CurrentLocation { get; set; }
    public List<string> Images { get; set; } = new();
    public AssetStatus Status { get; set; }
}

public class BorrowUserSummary
{
    public string Id { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? ImageUrl { get; set; }
}
