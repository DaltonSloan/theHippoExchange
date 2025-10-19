using HippoExchange.Models;
using HippoExchange.Api.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HippoExchange.Api.Services
{
    /// <summary>
    /// Provides CRUD operations for asset documents stored in MongoDB.
    /// </summary>
    public class AssetService
    {
        private readonly IMongoCollection<Assets> _assetsCollection;

        /// <summary>
        /// Creates a new <see cref="AssetService"/> instance using the shared database connection.
        /// </summary>
        public AssetService(IMongoDatabase database)
        {
            _assetsCollection = database.GetCollection<Assets>("assets");
        }

        /// <summary>
        /// Persists a new asset document.
        /// </summary>
        public async Task<Assets> CreateAssetAsync(Assets newAsset)
        {
            await _assetsCollection.InsertOneAsync(newAsset);
            return newAsset;
        }

        /// <summary>
        /// Retrieves all assets belonging to the specified owner.
        /// </summary>
        public async Task<List<Assets>> GetAssetsByOwnerIdAsync(string userId) =>
            await _assetsCollection.Find(a => a.OwnerUserId == userId).ToListAsync();

        /// <summary>
        /// Retrieves a single asset document by identifier.
        /// </summary>
        public async Task<Assets?> GetAssetByIdAsync(string assetId) =>
            await _assetsCollection.Find(a => a.Id == assetId).FirstOrDefaultAsync();

        /// <summary>
        /// Replaces an existing asset document with an updated payload.
        /// </summary>
        public async Task<bool> ReplaceAssetAsync(string assetId, Assets updatedAsset)
        {
            updatedAsset.Id = assetId;
            var result = await _assetsCollection.ReplaceOneAsync(a => a.Id == assetId, updatedAsset);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Deletes an asset document by identifier.
        /// </summary>
        public async Task<bool> DeleteAsset(string assetId)
        {
            var result = await _assetsCollection.DeleteOneAsync(a => a.Id == assetId);
            return result.DeletedCount > 0;
        }

        /// <summary>
        /// Updates the favorite flag for an asset.
        /// </summary>
        public async Task<bool> UpdateFavorite(string assetId, bool newValue)
        {
            var filter = Builders<Assets>.Filter.Eq(a => a.Id, assetId);
            var update = Builders<Assets>.Update.Set(a => a.Favorite, newValue);
            var result = await _assetsCollection.UpdateOneAsync(filter, update);
            return result.MatchedCount > 0;
        }

        /// <summary>
        /// Returns the stored image URLs for an asset.
        /// </summary>
        public async Task<List<string>?> GetAssetImage(string assetId)
        {
            var filter = Builders<Assets>.Filter.Eq(a => a.Id, assetId);

            // Only project (return) the Images field to minimize data load
            var projection = Builders<Assets>.Projection.Include(a => a.Images);

            // Find the asset with only its Images field
            var result = await _assetsCollection
                .Find(filter)
                .Project<Assets>(projection)
                .FirstOrDefaultAsync();

            // Return the image list p.s if I need this return to be null just in case do this result?.Images;
            return result?.Images;
        }
    }
}
