using HippoExchange.Models;
using HippoExchange.Api.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HippoExchange.Api.Services
{
    public class AssetService
    {
        private readonly IMongoCollection<Assets> _assetsCollection;

        public AssetService(IMongoDatabase database)
        {
            _assetsCollection = database.GetCollection<Assets>("assets");
        }

        public async Task<Assets> CreateAssetAsync(Assets newAsset)
        {
            await _assetsCollection.InsertOneAsync(newAsset);
            return newAsset;
        }

        public async Task<List<Assets>> GetAssetsByOwnerIdAsync(string userId) =>
            await _assetsCollection.Find(a => a.OwnerUserId == userId).ToListAsync();

        public async Task<Assets?> GetAssetByIdAsync(string assetId) =>
            await _assetsCollection.Find(a => a.Id == assetId).FirstOrDefaultAsync();

        public async Task<bool> ReplaceAssetAsync(string assetId, Assets updatedAsset)
        {
            updatedAsset.Id = assetId;
            var result = await _assetsCollection.ReplaceOneAsync(a => a.Id == assetId, updatedAsset);
            return result.ModifiedCount > 0;
        }
        public async Task<bool> DeleteAsset(string assetId)//this method is to delete a specified asset 
        {
            //delets the asset by comparing it with its id and returns meta data to use in error handeling
            var result = await _assetsCollection.DeleteOneAsync(a => a.Id == assetId);
            return result.DeletedCount > 0;//this will return true or false depending on if the delete is successful
        }

        public async Task<bool> UpdateFavorite(string assetId, bool newValue)
        {
            //builds the filter to find the asset
            var filter = Builders<Assets>.Filter.Eq(a => a.Id, assetId);
            //creates the update to put into database 
            var update = Builders<Assets>.Update.Set(a => a.Favorite, newValue);
            //does the update to the database 
            var result = await _assetsCollection.UpdateOneAsync(filter, update);
            //returns the results 
            return result.MatchedCount > 0;
        }
        //Method to get the image list for a given asset
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
