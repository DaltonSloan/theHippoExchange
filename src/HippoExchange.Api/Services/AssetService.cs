using HippoExchange.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HippoExchange.Services
{
    public class AssetService
    {
        private readonly IMongoCollection<Asset> _assetsCollection;

        public AssetService(IOptions<MongoSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _assetsCollection = mongoDatabase.GetCollection<Asset>("assets");
        }

        public async Task<Asset> CreateAssetAsync(Asset newAsset)
        {
            await _assetsCollection.InsertOneAsync(newAsset);
            return newAsset;
        }

        public async Task<List<Asset>> GetAssetsByOwnerIdAsync(string userId) =>
            await _assetsCollection.Find(a => a.OwnerUserId == userId).ToListAsync();

        public async Task<Asset?> GetAssetByIdAsync(string assetId) =>
            await _assetsCollection.Find(a => a.Id == assetId).FirstOrDefaultAsync();

        public async Task UpdateAssetAsync(string assetId, Asset updatedAsset) =>
            await _assetsCollection.ReplaceOneAsync(a => a.Id == assetId, updatedAsset);
    }
}
