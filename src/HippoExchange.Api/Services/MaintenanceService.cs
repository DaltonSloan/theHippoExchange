using HippoExchange.Models;
using HippoExchange.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HippoExchange.Api.Services
{
    public class MaintenanceService
    {
        private readonly IMongoCollection<Maintenance> _maintenanceCollection;

        public MaintenanceService(IOptions<MongoSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _maintenanceCollection = mongoDatabase.GetCollection<Maintenance>("maintenance");
        }

        // Create new maintenance record
        public async Task<Maintenance> CreateMaintenanceAsync(Maintenance record)
        {
            await _maintenanceCollection.InsertOneAsync(record);
            return record;
        }

        // Get all maintenance records for a given asset
        public async Task<List<Maintenance>> GetMaintenanceByAssetIdAsync(string assetId) =>
            await _maintenanceCollection.Find(m => m.AssetId == assetId).ToListAsync();

        public async Task<List<Maintenance>> GetMaintenanceByAssetIdsAsync(IEnumerable<string> assetIds)
        {
            var filter = Builders<Maintenance>.Filter.In(m => m.AssetId, assetIds);
            return await _maintenanceCollection.Find(filter).ToListAsync();
        }

        // Get a single maintenance record by ID
        public async Task<Maintenance?> GetMaintenanceByIdAsync(string id) =>
            await _maintenanceCollection.Find(m => m.Id == id).FirstOrDefaultAsync();

        // Update a maintenance record
        public async Task<bool> UpdateMaintenanceAsync(string id, Maintenance updatedRecord)
        {
            var result = await _maintenanceCollection.ReplaceOneAsync(m => m.Id == id, updatedRecord);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        // Delete a maintenance record
        public async Task<bool> DeleteMaintenanceAsync(string id)
        {
            var result = await _maintenanceCollection.DeleteOneAsync(m => m.Id == id);
            return result.DeletedCount > 0;
        }
    }
}