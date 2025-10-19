using HippoExchange.Models;
using HippoExchange.Api.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HippoExchange.Api.Services
{
    /// <summary>
    /// Provides persistence operations for maintenance records tied to assets.
    /// </summary>
    public class MaintenanceService
    {
        private readonly IMongoCollection<Maintenance> _maintenanceCollection;

        /// <summary>
        /// Creates a new <see cref="MaintenanceService"/> instance using the shared database connection.
        /// </summary>
        public MaintenanceService(IMongoDatabase database)
        {
            _maintenanceCollection = database.GetCollection<Maintenance>("maintenance");
        }

        /// <summary>
        /// Persists a new maintenance record.
        /// </summary>
        public async Task<Maintenance> CreateMaintenanceAsync(Maintenance record)
        {
            await _maintenanceCollection.InsertOneAsync(record);
            return record;
        }

        /// <summary>
        /// Retrieves maintenance records for a specific asset.
        /// </summary>
        public async Task<List<Maintenance>> GetMaintenanceByAssetIdAsync(string assetId) =>
            await _maintenanceCollection.Find(m => m.AssetId == assetId).ToListAsync();

        /// <summary>
        /// Retrieves maintenance records for multiple assets in a single query.
        /// </summary>
        public async Task<List<Maintenance>> GetMaintenanceByAssetIdsAsync(IEnumerable<string> assetIds)
        {
            var filter = Builders<Maintenance>.Filter.In(m => m.AssetId, assetIds);
            return await _maintenanceCollection.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Retrieves a single maintenance record by identifier.
        /// </summary>
        public async Task<Maintenance?> GetMaintenanceByIdAsync(string id) =>
            await _maintenanceCollection.Find(m => m.Id == id).FirstOrDefaultAsync();

        /// <summary>
        /// Overwrites a maintenance record with updated values.
        /// </summary>
        public async Task<bool> UpdateMaintenanceAsync(string id, Maintenance updatedRecord)
        {
            var filter = Builders<Maintenance>.Filter.Eq(m => m.Id, id);
            
            // The previous dynamic update was incomplete and led to data sync issues.
            // This new implementation ensures all fields from the request are updated,
            // creating a single source of truth from the client's request.
            var update = Builders<Maintenance>.Update
                .Set(m => m.MaintenanceTitle, updatedRecord.MaintenanceTitle)
                .Set(m => m.MaintenanceDescription, updatedRecord.MaintenanceDescription)
                .Set(m => m.MaintenanceDueDate, updatedRecord.MaintenanceDueDate)
                .Set(m => m.IsCompleted, updatedRecord.IsCompleted)
                .Set(m => m.PreserveFromPrior, updatedRecord.PreserveFromPrior)
                .Set(m => m.RequiredTools, updatedRecord.RequiredTools)
                .Set(m => m.RecurrenceInterval, updatedRecord.RecurrenceInterval)
                .Set(m => m.RecurrenceUnit, updatedRecord.RecurrenceUnit);

            var result = await _maintenanceCollection.UpdateOneAsync(filter, update);

            // Return true if the document was matched, even if no fields were modified.
            // This signals to the client that the save operation was successful.
            return result.IsAcknowledged && (result.ModifiedCount > 0 || result.MatchedCount > 0);
        }

        /// <summary>
        /// Deletes a maintenance record by identifier.
        /// </summary>
        public async Task<bool> DeleteMaintenanceAsync(string id)
        {
            var result = await _maintenanceCollection.DeleteOneAsync(m => m.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
