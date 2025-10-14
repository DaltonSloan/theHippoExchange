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
            var filter = Builders<Maintenance>.Filter.Eq(m => m.Id, id);
            var updateBuilder = Builders<Maintenance>.Update;
            var updateDefinition = new List<UpdateDefinition<Maintenance>>();

            // Dynamically build the update definition based on the provided fields
            if (updatedRecord.MaintenanceTitle != null)
                updateDefinition.Add(updateBuilder.Set(m => m.MaintenanceTitle, updatedRecord.MaintenanceTitle));
            if (updatedRecord.MaintenanceDescription != null)
                updateDefinition.Add(updateBuilder.Set(m => m.MaintenanceDescription, updatedRecord.MaintenanceDescription));
            if (updatedRecord.MaintenanceDueDate != default)
                updateDefinition.Add(updateBuilder.Set(m => m.MaintenanceDueDate, updatedRecord.MaintenanceDueDate));
            
            updateDefinition.Add(updateBuilder.Set(m => m.IsCompleted, updatedRecord.IsCompleted));
            updateDefinition.Add(updateBuilder.Set(m => m.PreserveFromPrior, updatedRecord.PreserveFromPrior));
            updateDefinition.Add(updateBuilder.Set(m => m.RecurrenceInterval, updatedRecord.RecurrenceInterval));
            updateDefinition.Add(updateBuilder.Set(m => m.RecurrenceUnit, updatedRecord.RecurrenceUnit));
            
            if (!updateDefinition.Any())
            {
                // Nothing to update
                return true;
            }

            var combinedUpdate = updateBuilder.Combine(updateDefinition);
            var result = await _maintenanceCollection.UpdateOneAsync(filter, combinedUpdate);
            
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