using HippoExchange.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HippoExchange.Services
{
    public class AssetService
    {
        private readonly IMongoCollection<Assets> _assetsCollection;

        public AssetService(IOptions<MongoSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _assetsCollection = mongoDatabase.GetCollection<Assets>("assets");
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
    }
}
