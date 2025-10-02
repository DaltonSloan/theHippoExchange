using HippoExchange.Models;
using HippoExchange.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HippoExchange.Services
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

        // Get a single maintenance record by ID
        public async Task<Maintenance?> GetMaintenanceByIdAsync(string maintenanceId) =>
            await _maintenanceCollection.Find(m => m.Id == maintenanceId).FirstOrDefaultAsync();

        // Get all maintenance records (for "all maintenance" page)
        public async Task<List<Maintenance>> GetAllMaintenanceAsync() =>
            await _maintenanceCollection.Find(_ => true).ToListAsync();

        // Update a maintenance record (full replace)
        public async Task<bool> UpdateMaintenanceAsync(string maintenanceId, Maintenance updatedRecord)
        {
            updatedRecord.Id = maintenanceId;
            var result = await _maintenanceCollection.ReplaceOneAsync(m => m.Id == maintenanceId, updatedRecord);
            return result.ModifiedCount > 0;
        }

        // Delete a maintenance record
        public async Task<bool> DeleteMaintenanceAsync(string maintenanceId)
        {
            var result = await _maintenanceCollection.DeleteOneAsync(m => m.Id == maintenanceId);
            return result.DeletedCount > 0;
        }
    }
}