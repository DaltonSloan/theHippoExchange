using HippoExchange.Api.Models;
using HippoExchange.Api.Utilities;
using HippoExchange.Models;
using MongoDB.Driver;

namespace HippoExchange.Api.Services;

/// <summary>
/// Coordinates creation and lifecycle management of borrow requests between borrowers and owners.
/// </summary>
public class BorrowService
{
    private readonly IMongoCollection<BorrowRequest> _borrowCollection;
    private readonly IMongoCollection<Assets> _assetsCollection;
    private readonly UserService _userService;

    public BorrowService(IMongoDatabase database, UserService userService)
    {
        _borrowCollection = database.GetCollection<BorrowRequest>("borrowRequests");
        _assetsCollection = database.GetCollection<Assets>("assets");
        _userService = userService;
    }

    public async Task<BorrowRequest?> GetByIdAsync(string requestId) =>
        await _borrowCollection.Find(r => r.Id == requestId).FirstOrDefaultAsync();

    public async Task<BorrowRequest> CreateRequestAsync(CreateBorrowRequest request, string borrowerUserId)
    {
        var asset = await _assetsCollection.Find(a => a.Id == request.AssetId).FirstOrDefaultAsync();
        if (asset is null)
        {
            throw new InvalidOperationException("Asset not found.");
        }

        if (asset.OwnerUserId == borrowerUserId)
        {
            throw new InvalidOperationException("You cannot borrow your own asset.");
        }

        if (asset.Status == AssetStatus.Borrowed)
        {
            throw new InvalidOperationException("Asset is currently borrowed.");
        }

        var borrowRequest = new BorrowRequest
        {
            AssetId = asset.Id!,
            OwnerUserId = asset.OwnerUserId,
            BorrowerUserId = borrowerUserId,
            RequestedFrom = request.RequestedFrom?.ToUniversalTime(),
            RequestedUntil = request.RequestedUntil?.ToUniversalTime(),
            Message = request.Message
        };

        borrowRequest = InputSanitizer.SanitizeObject(borrowRequest);

        await _borrowCollection.InsertOneAsync(borrowRequest);
        return borrowRequest;
    }

    public async Task<BorrowRequest?> DecideAsync(string requestId, string ownerUserId, BorrowDecisionRequest decision)
    {
        var borrowRequest = await _borrowCollection.Find(r => r.Id == requestId).FirstOrDefaultAsync();
        if (borrowRequest is null)
        {
            return null;
        }

        if (!string.Equals(borrowRequest.OwnerUserId, ownerUserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("You do not own this asset.");
        }

        if (borrowRequest.Status != BorrowRequestStatus.Pending)
        {
            return borrowRequest;
        }

        var newStatus = decision.Approve ? BorrowRequestStatus.Approved : BorrowRequestStatus.Denied;
        var update = Builders<BorrowRequest>.Update
            .Set(r => r.Status, newStatus)
            .Set(r => r.ReviewedAt, DateTime.UtcNow)
            .Set(r => r.OwnerNote, InputSanitizer.Clean(decision.Note))
            .Set(r => r.DueAt, decision.DueAt?.ToUniversalTime());

        await _borrowCollection.UpdateOneAsync(r => r.Id == requestId, update);

        if (decision.Approve)
        {
            await _assetsCollection.UpdateOneAsync(
                a => a.Id == borrowRequest.AssetId,
                Builders<Assets>.Update.Set(a => a.Status, AssetStatus.Borrowed));
        }

        return await GetByIdAsync(requestId);
    }

    public async Task<BorrowRequest?> CompleteAsync(string requestId, string ownerUserId, string? note)
    {
        var borrowRequest = await _borrowCollection.Find(r => r.Id == requestId).FirstOrDefaultAsync();
        if (borrowRequest is null)
        {
            return null;
        }

        if (!string.Equals(borrowRequest.OwnerUserId, ownerUserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("You do not own this asset.");
        }

        if (borrowRequest.Status != BorrowRequestStatus.Approved)
        {
            return borrowRequest;
        }

        var update = Builders<BorrowRequest>.Update
            .Set(r => r.Status, BorrowRequestStatus.Returned)
            .Set(r => r.ReturnedAt, DateTime.UtcNow)
            .Set(r => r.OwnerNote, InputSanitizer.Clean(note));

        await _borrowCollection.UpdateOneAsync(r => r.Id == requestId, update);

        await _assetsCollection.UpdateOneAsync(
            a => a.Id == borrowRequest.AssetId,
            Builders<Assets>.Update.Set(a => a.Status, AssetStatus.Available));

        return await GetByIdAsync(requestId);
    }

    public async Task<List<BorrowRequestSummary>> GetBorrowerSummariesAsync(string borrowerUserId) =>
        await BuildSummariesAsync(await _borrowCollection
                .Find(r => r.BorrowerUserId == borrowerUserId)
                .SortByDescending(r => r.RequestedAt)
                .ToListAsync(),
            includeOwner: true);

    public async Task<List<BorrowRequestSummary>> GetOwnerSummariesAsync(string ownerUserId) =>
        await BuildSummariesAsync(await _borrowCollection
                .Find(r => r.OwnerUserId == ownerUserId)
                .SortByDescending(r => r.RequestedAt)
                .ToListAsync(),
            includeOwner: false);

    private async Task<List<BorrowRequestSummary>> BuildSummariesAsync(
        List<BorrowRequest> requests,
        bool includeOwner)
    {
        if (requests.Count == 0)
        {
            return new List<BorrowRequestSummary>();
        }

        var assetIds = requests.Select(r => r.AssetId).Distinct().ToList();
        var assets = await _assetsCollection.Find(a => assetIds.Contains(a.Id!)).ToListAsync();
        var assetMap = assets.ToDictionary(a => a.Id!, a => a);

        var userIds = includeOwner
            ? requests.Select(r => r.OwnerUserId)
            : requests.Select(r => r.BorrowerUserId);
        var users = await _userService.GetUsersByIdsAsync(userIds);

        var summaries = new List<BorrowRequestSummary>();

        foreach (var request in requests)
        {
            assetMap.TryGetValue(request.AssetId, out var asset);
            var counterpartyId = includeOwner ? request.OwnerUserId : request.BorrowerUserId;
            users.TryGetValue(counterpartyId, out var counterparty);

            var assetSummary = asset is null
                ? new BorrowAssetSummary
                {
                    Id = request.AssetId,
                    ItemName = "Asset unavailable",
                    Images = new List<string>(),
                    Status = AssetStatus.Unlisted
                }
                : new BorrowAssetSummary
                {
                    Id = asset.Id!,
                    ItemName = asset.ItemName,
                    BrandName = asset.BrandName,
                    Category = asset.Category,
                    CurrentLocation = asset.CurrentLocation,
                    Images = asset.Images ?? new List<string>(),
                    Status = asset.Status
                };

            BorrowUserSummary userSummary;
            if (counterparty is null)
            {
                userSummary = new BorrowUserSummary
                {
                    Id = counterpartyId,
                    Username = "Unknown user"
                };
            }
            else
            {
                userSummary = new BorrowUserSummary
                {
                    Id = counterparty.ClerkId,
                    FirstName = counterparty.FirstName,
                    LastName = counterparty.LastName,
                    Username = counterparty.Username,
                    ImageUrl = counterparty.ImageUrl ?? counterparty.ProfileImageUrl
                };
            }

            summaries.Add(new BorrowRequestSummary
            {
                Request = request,
                Asset = assetSummary,
                Counterparty = userSummary
            });
        }

        return summaries;
    }
}
