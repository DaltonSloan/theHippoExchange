# HippoExchange Maintenance Endpoints - Comprehensive Guide

## Table of Contents
1. [Overview](#overview)
2. [Data Model](#data-model)
3. [Available Endpoints](#available-endpoints)
4. [Detailed Endpoint Usage](#detailed-endpoint-usage)
5. [Integration with Assets](#integration-with-assets)
6. [Common Use Cases](#common-use-cases)
7. [Potential Problems & Solutions](#potential-problems--solutions)
8. [Best Practices](#best-practices)
9. [Example Request/Response Flows](#example-requestresponse-flows)

---

## Overview

The maintenance endpoints in HippoExchange API allow users to track maintenance records for their assets (home goods, equipment, appliances, etc.). These endpoints enable creating, retrieving, updating, and deleting maintenance schedules and histories for individual assets.

### Key Features
- **Asset-linked maintenance tracking**: Each maintenance record is associated with a specific asset
- **Product information preservation**: Store brand, product name, and purchase details
- **Scheduling capabilities**: Track when maintenance is due
- **Tool management**: Document required tools and their locations
- **Status tracking**: Monitor pending vs. completed maintenance
- **Historical preservation**: Option to preserve maintenance records from prior maintenance cycles

---

## Data Model

### Maintenance Object Schema

```csharp
public class Maintenance
{
    // MongoDB identifier (auto-generated)
    public string? Id { get; set; }
    
    // Required: References the Asset this maintenance belongs to
    public string AssetId { get; set; }
    
    // Product identification
    public string BrandName { get; set; }
    public string ProductName { get; set; }
    
    // Purchase tracking
    public string PurchaseLocation { get; set; }  // e.g., "Lowes", "Home Depot"
    public decimal CostPaid { get; set; }
    
    // Maintenance scheduling
    public DateTime MaintenanceDueDate { get; set; }
    public string MaintenanceTitle { get; set; }
    public string MaintenanceDescription { get; set; }
    public string MaintenanceStatus { get; set; }  // "pending" or "completed"
    
    // Historical tracking
    public bool PreserveFromPrior { get; set; }
    
    // Tool management
    public string RequiredTools { get; set; }
    public string ToolLocation { get; set; }
}
```

### Field Details

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `Id` | string | No (auto-generated) | null | MongoDB ObjectId identifier |
| `AssetId` | string | **Yes** | Empty string | Reference to parent Asset |
| `BrandName` | string | No | Empty string | Brand/manufacturer name |
| `ProductName` | string | No | Empty string | Product/model name |
| `PurchaseLocation` | string | No | Empty string | Where the item was purchased |
| `CostPaid` | decimal | No | 0 | Purchase cost |
| `MaintenanceDueDate` | DateTime | **Yes** | - | When maintenance is due |
| `MaintenanceTitle` | string | No | Empty string | Short title for the maintenance task |
| `MaintenanceDescription` | string | No | Empty string | Detailed description of maintenance work |
| `MaintenanceStatus` | string | No | "pending" | Status: "pending" or "completed" |
| `PreserveFromPrior` | bool | No | false | Whether to keep this record from previous maintenance |
| `RequiredTools` | string | No | Empty string | List/description of tools needed |
| `ToolLocation` | string | No | Empty string | Where the tools are located |

---

## Available Endpoints

### Summary Table

| Method | Endpoint | Purpose | Auth Required |
|--------|----------|---------|---------------|
| POST | `/api/assets/{assetId}/maintenance` | Create new maintenance record | No* |
| GET | `/api/assets/{assetId}/maintenance` | Get all maintenance for an asset | No* |
| GET | `/api/maintenace` | Get all maintenance records (all assets) | No* |
| PUT | *(Not implemented)* | Update a maintenance record | - |
| DELETE | *(Not implemented)* | Delete a maintenance record | - |
| GET | *(Not implemented)* | Get single maintenance by ID | - |

**Note**: While the service layer has methods for Update, Delete, and GetById operations, these are **not currently exposed** as API endpoints in `Program.cs`.

\* Current authentication is placeholder-based (X-User-Id header). Endpoints don't currently validate user ownership.

---

## Detailed Endpoint Usage

### 1. POST `/api/assets/{assetId}/maintenance`

**Purpose**: Create a new maintenance record for a specific asset.

**Route Parameters**:
- `assetId` (string, required): The ObjectId of the asset

**Request Body** (JSON):
```json
{
  "brandName": "Whirlpool",
  "productName": "Model WRF555SDFZ",
  "purchaseLocation": "Lowes",
  "costPaid": 1299.99,
  "maintenanceDueDate": "2025-12-01T00:00:00Z",
  "maintenanceTitle": "Replace Water Filter",
  "maintenanceDescription": "Replace refrigerator water filter with model EveryDrop 4",
  "maintenanceStatus": "pending",
  "preserveFromPrior": false,
  "requiredTools": "None - hand removal",
  "toolLocation": "N/A"
}
```

**Validation**:
- AssetId cannot be null, empty, or whitespace
- The request automatically sets `maintenance.AssetId = assetId` from the route parameter

**Response**:
- **201 Created** on success
- Location header: `/api/maintenance/{created.Id}`
- Response body: The created maintenance object with generated `Id`

**Example Response**:
```json
{
  "id": "671234567890abcdef123456",
  "assetId": "670abcdef1234567890abcde",
  "brandName": "Whirlpool",
  "productName": "Model WRF555SDFZ",
  "purchaseLocation": "Lowes",
  "costPaid": 1299.99,
  "maintenanceDueDate": "2025-12-01T00:00:00Z",
  "maintenanceTitle": "Replace Water Filter",
  "maintenanceDescription": "Replace refrigerator water filter with model EveryDrop 4",
  "maintenanceStatus": "pending",
  "preserveFromPrior": false,
  "requiredTools": "None - hand removal",
  "toolLocation": "N/A"
}
```

**Error Responses**:
- **400 Bad Request**: If `assetId` is null/empty
- **500 Internal Server Error**: Database connection issues

---

### 2. GET `/api/assets/{assetId}/maintenance`

**Purpose**: Retrieve all maintenance records associated with a specific asset.

**Route Parameters**:
- `assetId` (string, required): The ObjectId of the asset

**Query Parameters**: None

**Response**:
- **200 OK** with array of maintenance records
- Empty array `[]` if no maintenance records exist for the asset

**Example Response**:
```json
[
  {
    "id": "671234567890abcdef123456",
    "assetId": "670abcdef1234567890abcde",
    "brandName": "Whirlpool",
    "productName": "Model WRF555SDFZ",
    "maintenanceTitle": "Replace Water Filter",
    "maintenanceDueDate": "2025-12-01T00:00:00Z",
    "maintenanceStatus": "pending"
  },
  {
    "id": "671234567890abcdef123457",
    "assetId": "670abcdef1234567890abcde",
    "brandName": "Whirlpool",
    "productName": "Model WRF555SDFZ",
    "maintenanceTitle": "Clean Coils",
    "maintenanceDueDate": "2026-03-15T00:00:00Z",
    "maintenanceStatus": "completed"
  }
]
```

**Use Case**: Display all maintenance tasks for a specific appliance or item.

---

### 3. GET `/api/maintenace`

**Purpose**: Retrieve ALL maintenance records across all assets (system-wide view).

⚠️ **WARNING**: Notice the typo in the endpoint: `maintenace` instead of `maintenance`

**Query Parameters**: None

**Response**:
- **200 OK** with array of ALL maintenance records in the database
- Empty array `[]` if no maintenance records exist

**Example Response**:
```json
[
  {
    "id": "671234567890abcdef123456",
    "assetId": "670abcdef1234567890abcde",
    "brandName": "Whirlpool",
    "productName": "Refrigerator",
    "maintenanceTitle": "Replace Water Filter",
    "maintenanceDueDate": "2025-12-01T00:00:00Z",
    "maintenanceStatus": "pending"
  },
  {
    "id": "671234567890abcdef123458",
    "assetId": "670abcdef1234567890abcdf",
    "brandName": "Craftsman",
    "productName": "Lawn Mower",
    "maintenanceTitle": "Oil Change",
    "maintenanceDueDate": "2025-05-01T00:00:00Z",
    "maintenanceStatus": "pending"
  }
]
```

**Use Case**: 
- Dashboard showing all upcoming maintenance across all assets
- Calendar view of maintenance schedule
- Admin/overview pages

**Security Concern**: This endpoint returns ALL maintenance records for ALL users with no filtering by `X-User-Id`. This is a potential security/privacy issue.

---

### 4. Service Layer Methods (Not Exposed as Endpoints)

The `MaintenanceService` class includes additional methods that are **NOT currently accessible** via API endpoints:

#### `GetMaintenanceByIdAsync(string maintenanceId)`
- Retrieves a single maintenance record by its ID
- Returns `null` if not found

#### `UpdateMaintenanceAsync(string maintenanceId, Maintenance updatedRecord)`
- Full replacement of a maintenance record
- Returns `true` if modified, `false` if not found
- Sets `updatedRecord.Id = maintenanceId` before replacement

#### `DeleteMaintenanceAsync(string maintenanceId)`
- Deletes a maintenance record
- Returns `true` if deleted, `false` if not found

**To use these methods**, you would need to add corresponding endpoint mappings in `Program.cs`.

---

## Integration with Assets

### Asset-Maintenance Relationship

Maintenance records are linked to assets via the `AssetId` field. The relationship is:

```
Asset (1) ──── (*) Maintenance
```

One asset can have many maintenance records, but each maintenance record belongs to exactly one asset.

### Asset Model Reference

```csharp
public class Asset
{
    public string? Id { get; set; }
    public string ItemName { get; set; }
    public string BrandName { get; set; }
    public string Category { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public string CurrentLocation { get; set; }
    public List<string> Images { get; set; }
    public string ConditionDescription { get; set; }
    public string OwnerUserId { get; set; }
    public string Status { get; set; }  // "available", "loaned", "maintenance"
    public bool Favorite { get; set; }
}
```

### Workflow Integration

**Typical Flow**:
1. User creates an asset via `POST /api/assets`
2. System returns the asset with generated `Id`
3. User schedules maintenance via `POST /api/assets/{assetId}/maintenance`
4. User can optionally update asset status to `"maintenance"` via `PUT /api/assets/{assetId}`

**Example**:
```bash
# Step 1: Create an asset
POST /api/assets
{
  "itemName": "Refrigerator",
  "brandName": "Whirlpool",
  "category": "Appliances",
  "purchaseDate": "2023-01-15T00:00:00Z",
  "purchaseCost": 1299.99,
  "status": "available"
}

# Response: { "id": "670abcdef1234567890abcde", ... }

# Step 2: Add maintenance record
POST /api/assets/670abcdef1234567890abcde/maintenance
{
  "maintenanceTitle": "Filter Replacement",
  "maintenanceDueDate": "2025-12-01T00:00:00Z",
  "maintenanceStatus": "pending"
}

# Step 3: Update asset status (optional)
PUT /api/assets/670abcdef1234567890abcde
{
  "status": "maintenance",
  ...other asset fields...
}
```

---

## Common Use Cases

### 1. Creating a Recurring Maintenance Schedule

**Scenario**: User wants to schedule filter replacements every 6 months.

**Approach**: Create multiple maintenance records with different due dates.

```json
// First maintenance
POST /api/assets/{assetId}/maintenance
{
  "maintenanceTitle": "Filter Replacement - Cycle 1",
  "maintenanceDueDate": "2025-06-01T00:00:00Z",
  "maintenanceStatus": "pending"
}

// Second maintenance
POST /api/assets/{assetId}/maintenance
{
  "maintenanceTitle": "Filter Replacement - Cycle 2",
  "maintenanceDueDate": "2025-12-01T00:00:00Z",
  "maintenanceStatus": "pending"
}
```

### 2. Marking Maintenance as Complete

**Current Limitation**: No direct UPDATE endpoint exposed.

**Workaround**: The service layer has `UpdateMaintenanceAsync()`, but you'd need to add an endpoint:

```csharp
// Add to Program.cs (not currently present)
app.MapPut("/api/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    string maintenanceId,
    Maintenance updatedMaintenance) =>
{
    var success = await maintenanceService.UpdateMaintenanceAsync(maintenanceId, updatedMaintenance);
    return success ? Results.Ok(updatedMaintenance) : Results.NotFound();
});
```

### 3. Dashboard View of All Upcoming Maintenance

**Request**:
```bash
GET /api/maintenace
```

**Client-side filtering** for upcoming maintenance:
```javascript
const today = new Date();
const upcomingMaintenance = allMaintenance
  .filter(m => m.maintenanceStatus === 'pending')
  .filter(m => new Date(m.maintenanceDueDate) >= today)
  .sort((a, b) => new Date(a.maintenanceDueDate) - new Date(b.maintenanceDueDate));
```

### 4. Tracking Tool Requirements Across Assets

**Request**: Get all maintenance records
```bash
GET /api/maintenace
```

**Client-side aggregation**:
```javascript
const toolsNeeded = allMaintenance
  .filter(m => m.maintenanceStatus === 'pending')
  .reduce((acc, m) => {
    if (m.requiredTools && m.requiredTools !== 'None') {
      acc.push({
        tool: m.requiredTools,
        location: m.toolLocation,
        forAsset: m.assetId,
        task: m.maintenanceTitle
      });
    }
    return acc;
  }, []);
```

### 5. Maintenance History Preservation

**Scenario**: User completes maintenance but wants to preserve the record.

**Using `PreserveFromPrior`**:
```json
// Mark current maintenance as complete
PUT /api/maintenance/{oldId}
{
  "maintenanceStatus": "completed",
  "preserveFromPrior": true,
  ...other fields...
}

// Create new maintenance cycle
POST /api/assets/{assetId}/maintenance
{
  "maintenanceTitle": "Filter Replacement - Next Cycle",
  "maintenanceDueDate": "2026-06-01T00:00:00Z",
  "maintenanceStatus": "pending",
  "preserveFromPrior": false
}
```

---

## Potential Problems & Solutions

### 1. **URL Typo in Global Maintenance Endpoint**

**Problem**: The endpoint is spelled `/api/maintenace` instead of `/api/maintenance`.

**Impact**: 
- Developers might waste time debugging 404 errors
- API inconsistency
- Client code needs to use the misspelled URL

**Solution**:
```csharp
// In Program.cs, change line 215:
app.MapGet("/api/maintenance", async ( // Fixed spelling
    [FromServices] MaintenanceService maintenanceService) =>
    {
        var records = await maintenanceService.GetAllMaintenanceAsync();
        return Results.Ok(records);
    });
```

**Migration Strategy**: 
- If clients already use the misspelled endpoint, add both:
```csharp
var getAllMaintenanceHandler = async ([FromServices] MaintenanceService maintenanceService) =>
{
    var records = await maintenanceService.GetAllMaintenanceAsync();
    return Results.Ok(records);
};

app.MapGet("/api/maintenace", getAllMaintenanceHandler); // Old (typo)
app.MapGet("/api/maintenance", getAllMaintenanceHandler); // Correct
```

---

### 2. **Missing Update and Delete Endpoints**

**Problem**: Service has `UpdateMaintenanceAsync()` and `DeleteMaintenanceAsync()`, but no API endpoints expose them.

**Impact**:
- Cannot mark maintenance as complete via API
- Cannot delete incorrect/cancelled maintenance records
- Must modify database directly or rebuild service layer

**Solution**: Add these endpoints to `Program.cs`:

```csharp
// PUT /api/maintenance/{maintenanceId}
app.MapPut("/api/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    string maintenanceId,
    Maintenance updatedMaintenance) =>
{
    if (string.IsNullOrWhiteSpace(maintenanceId))
        return Results.BadRequest("Maintenance ID required");

    var existing = await maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
    if (existing is null)
        return Results.NotFound($"Maintenance record {maintenanceId} not found");

    var success = await maintenanceService.UpdateMaintenanceAsync(maintenanceId, updatedMaintenance);
    return success ? Results.Ok(updatedMaintenance) : Results.Problem("Failed to update maintenance");
});

// DELETE /api/maintenance/{maintenanceId}
app.MapDelete("/api/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    string maintenanceId) =>
{
    if (string.IsNullOrWhiteSpace(maintenanceId))
        return Results.BadRequest("Maintenance ID required");

    var success = await maintenanceService.DeleteMaintenanceAsync(maintenanceId);
    return success ? Results.NoContent() : Results.NotFound();
});

// GET /api/maintenance/{maintenanceId}
app.MapGet("/api/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    string maintenanceId) =>
{
    var record = await maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
    return record is not null ? Results.Ok(record) : Results.NotFound();
});
```

---

### 3. **No Authentication/Authorization on Maintenance Endpoints**

**Problem**: 
- `GET /api/maintenace` returns ALL maintenance for ALL users
- No validation that the user owns the asset they're adding maintenance to
- No `X-User-Id` header check like other endpoints

**Impact**:
- Privacy leak: Users can see other users' maintenance schedules
- Security risk: Users could add maintenance to assets they don't own
- Data pollution: Malicious users could spam maintenance records

**Current State**:
```csharp
// No auth check!
app.MapPost("/api/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    string assetId,
    Maintenance maintenance) => { ... });
```

**Solution**: Add authentication and ownership validation:

```csharp
app.MapPost("/api/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx,
    string assetId,
    Maintenance maintenance) =>
{
    // Get authenticated user
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId))
        return Results.Unauthorized();

    if (string.IsNullOrWhiteSpace(assetId))
        return Results.BadRequest("Asset ID required");

    // Verify user owns this asset
    var asset = await assetService.GetAssetByIdAsync(assetId);
    if (asset is null)
        return Results.NotFound($"Asset {assetId} not found");
    
    if (asset.OwnerUserId != userId)
        return Results.Forbid();

    maintenance.AssetId = assetId;
    var created = await maintenanceService.CreateMaintenanceAsync(maintenance);
    return Results.Created($"/api/maintenance/{created.Id}", created);
});

// Filter global maintenance by user
app.MapGet("/api/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId))
        return Results.Unauthorized();

    var allMaintenance = await maintenanceService.GetAllMaintenanceAsync();
    var userAssets = await assetService.GetAssetsByOwnerIdAsync(userId);
    var userAssetIds = userAssets.Select(a => a.Id).ToHashSet();

    var userMaintenance = allMaintenance
        .Where(m => userAssetIds.Contains(m.AssetId))
        .ToList();

    return Results.Ok(userMaintenance);
});
```

**Better Approach**: Add filtering at the service layer:

```csharp
// In MaintenanceService.cs
public async Task<List<Maintenance>> GetMaintenanceByOwnerIdAsync(string userId, AssetService assetService)
{
    var userAssets = await assetService.GetAssetsByOwnerIdAsync(userId);
    var assetIds = userAssets.Select(a => a.Id).ToList();
    
    return await _maintenanceCollection
        .Find(m => assetIds.Contains(m.AssetId))
        .ToListAsync();
}
```

---

### 4. **No Asset Validation on Creation**

**Problem**: When creating maintenance via `POST /api/assets/{assetId}/maintenance`, there's no check that the asset actually exists.

**Impact**:
- Can create maintenance for non-existent assets
- Orphaned maintenance records
- Database integrity issues

**Solution**: Add asset existence validation:

```csharp
app.MapPost("/api/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,  // Add this
    string assetId,
    Maintenance maintenance) =>
{
    if (string.IsNullOrWhiteSpace(assetId))
        return Results.BadRequest("Asset ID required");

    // Validate asset exists
    var asset = await assetService.GetAssetByIdAsync(assetId);
    if (asset is null)
        return Results.NotFound($"Asset {assetId} not found");

    maintenance.AssetId = assetId;
    var created = await maintenanceService.CreateMaintenanceAsync(maintenance);
    return Results.Created($"/api/maintenance/{created.Id}", created);
});
```

---

### 5. **MongoDB ObjectId Validation**

**Problem**: If an invalid ObjectId string is passed as `assetId` or `maintenanceId`, MongoDB will throw an exception.

**Example**: `GET /api/assets/invalid-id/maintenance`

**Impact**: 
- 500 Internal Server Error instead of 400 Bad Request
- Poor error messages for clients
- Logs cluttered with exceptions

**Solution**: Add ObjectId validation:

```csharp
using MongoDB.Bson;

bool IsValidObjectId(string id) => ObjectId.TryParse(id, out _);

app.MapPost("/api/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    string assetId,
    Maintenance maintenance) =>
{
    if (string.IsNullOrWhiteSpace(assetId))
        return Results.BadRequest("Asset ID required");

    if (!IsValidObjectId(assetId))
        return Results.BadRequest($"Invalid Asset ID format: {assetId}");

    // ... rest of logic
});
```

---

### 6. **Date Handling and Time Zones**

**Problem**: `MaintenanceDueDate` is a `DateTime` type, which can cause time zone confusion.

**Scenarios**:
- Client sends `"2025-12-01"` (no time/timezone)
- Server interprets as UTC midnight
- User sees different date based on their timezone

**Example Issue**:
```json
// Client sends:
{ "maintenanceDueDate": "2025-12-01" }

// Server stores:
2025-12-01T00:00:00Z (UTC)

// User in PST sees:
2025-11-30T16:00:00-08:00 (previous day!)
```

**Solutions**:

**Option 1**: Use UTC consistently and document it:
```csharp
// Always parse/return as UTC
maintenance.MaintenanceDueDate = DateTime.SpecifyKind(
    maintenance.MaintenanceDueDate, 
    DateTimeKind.Utc
);
```

**Option 2**: Use dates only (no time component):
```csharp
// Store only the date portion
maintenance.MaintenanceDueDate = maintenance.MaintenanceDueDate.Date;
```

**Option 3**: Store user's timezone with each user profile and convert accordingly.

**Recommendation**: For maintenance due dates, Option 2 (date-only) is simplest since exact time usually doesn't matter.

---

### 7. **Cascade Deletion Issues**

**Problem**: If an asset is deleted, its associated maintenance records remain orphaned in the database.

**Impact**:
- Database pollution
- `GET /api/maintenace` returns maintenance for deleted assets
- Storage waste

**Solution**: Implement cascade deletion:

```csharp
// In AssetService.cs (or wherever asset deletion is handled)
public async Task<bool> DeleteAssetAsync(string assetId)
{
    // Delete associated maintenance records first
    var maintenanceFilter = Builders<Maintenance>.Filter.Eq(m => m.AssetId, assetId);
    await _maintenanceCollection.DeleteManyAsync(maintenanceFilter);

    // Then delete the asset
    var result = await _assetCollection.DeleteOneAsync(a => a.Id == assetId);
    return result.DeletedCount > 0;
}
```

**Alternative**: Add a database reference constraint or use MongoDB's `$lookup` aggregation.

---

### 8. **Missing Input Validation**

**Problem**: No validation on required fields or data formats.

**Examples of issues**:
- Negative `CostPaid` values
- `MaintenanceDueDate` in the past
- Empty `MaintenanceTitle` causing confusion
- `MaintenanceStatus` accepting any string (e.g., "DONE" instead of "completed")

**Solution**: Add validation attributes or manual validation:

```csharp
// Option 1: Data annotations
public class Maintenance
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string MaintenanceTitle { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal CostPaid { get; set; }

    [RegularExpression("^(pending|completed)$")]
    public string MaintenanceStatus { get; set; } = "pending";
}

// Option 2: Manual validation in endpoint
app.MapPost("/api/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    string assetId,
    Maintenance maintenance) =>
{
    // Validate status
    if (maintenance.MaintenanceStatus != "pending" && 
        maintenance.MaintenanceStatus != "completed")
    {
        return Results.BadRequest("Status must be 'pending' or 'completed'");
    }

    // Validate cost
    if (maintenance.CostPaid < 0)
    {
        return Results.BadRequest("Cost cannot be negative");
    }

    // Validate title
    if (string.IsNullOrWhiteSpace(maintenance.MaintenanceTitle))
    {
        return Results.BadRequest("Maintenance title is required");
    }

    // ... continue with creation
});
```

---

### 9. **No Pagination for Large Result Sets**

**Problem**: `GET /api/maintenace` and `GET /api/assets/{assetId}/maintenance` return ALL records with no pagination.

**Impact**:
- Performance issues with large datasets
- Slow API responses
- High memory usage
- Poor mobile experience

**Solution**: Add pagination parameters:

```csharp
app.MapGet("/api/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    HttpContext ctx,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20) =>
{
    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 20;

    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId))
        return Results.Unauthorized();

    var allRecords = await maintenanceService.GetAllMaintenanceAsync();
    
    var pagedRecords = allRecords
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var response = new
    {
        page,
        pageSize,
        total = allRecords.Count,
        totalPages = (int)Math.Ceiling(allRecords.Count / (double)pageSize),
        data = pagedRecords
    };

    return Results.Ok(response);
});
```

**Better Performance**: Implement pagination at the database level:

```csharp
// In MaintenanceService.cs
public async Task<(List<Maintenance> items, long total)> GetMaintenancePagedAsync(
    int page, 
    int pageSize)
{
    var total = await _maintenanceCollection.CountDocumentsAsync(_ => true);
    var items = await _maintenanceCollection
        .Find(_ => true)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();
    
    return (items, total);
}
```

---

### 10. **No Sorting or Filtering Options**

**Problem**: Cannot sort by due date, filter by status, or search by title without client-side processing.

**Impact**:
- All filtering/sorting done client-side
- Inefficient for large datasets
- Poor user experience

**Solution**: Add query parameters for filtering and sorting:

```csharp
app.MapGet("/api/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromQuery] string? status = null,
    [FromQuery] string? sortBy = "maintenanceDueDate",
    [FromQuery] string? sortOrder = "asc") =>
{
    var records = await maintenanceService.GetAllMaintenanceAsync();

    // Filter by status
    if (!string.IsNullOrWhiteSpace(status))
    {
        records = records.Where(m => 
            m.MaintenanceStatus.Equals(status, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }

    // Sort
    records = sortBy?.ToLower() switch
    {
        "duedate" => sortOrder == "desc"
            ? records.OrderByDescending(m => m.MaintenanceDueDate).ToList()
            : records.OrderBy(m => m.MaintenanceDueDate).ToList(),
        "title" => sortOrder == "desc"
            ? records.OrderByDescending(m => m.MaintenanceTitle).ToList()
            : records.OrderBy(m => m.MaintenanceTitle).ToList(),
        "status" => sortOrder == "desc"
            ? records.OrderByDescending(m => m.MaintenanceStatus).ToList()
            : records.OrderBy(m => m.MaintenanceStatus).ToList(),
        _ => records.OrderBy(m => m.MaintenanceDueDate).ToList()
    };

    return Results.Ok(records);
});
```

**Example Usage**:
```
GET /api/maintenance?status=pending&sortBy=duedate&sortOrder=asc
```

---

## Best Practices

### 1. **Always Validate AssetId Before Creating Maintenance**

```csharp
// Check asset exists and user owns it
var asset = await assetService.GetAssetByIdAsync(assetId);
if (asset is null) return Results.NotFound("Asset not found");
if (asset.OwnerUserId != userId) return Results.Forbid();
```

### 2. **Use Consistent Date Formats**

Always use ISO 8601 format for dates:
```json
{ "maintenanceDueDate": "2025-12-01T00:00:00Z" }
```

### 3. **Set Default Status Explicitly**

```csharp
if (string.IsNullOrWhiteSpace(maintenance.MaintenanceStatus))
{
    maintenance.MaintenanceStatus = "pending";
}
```

### 4. **Update Asset Status When Adding Maintenance**

```csharp
// When creating maintenance, optionally update asset status
if (asset.Status != "maintenance")
{
    asset.Status = "maintenance";
    await assetService.ReplaceAssetAsync(assetId, asset);
}
```

### 5. **Validate ObjectIds Early**

```csharp
if (!ObjectId.TryParse(assetId, out _))
{
    return Results.BadRequest("Invalid Asset ID format");
}
```

### 6. **Return Meaningful Error Messages**

```csharp
// Bad
return Results.Problem();

// Good
return Results.Problem(
    detail: "Failed to create maintenance record. Database connection error.",
    statusCode: 500
);
```

### 7. **Use DTOs for Request/Response**

Instead of exposing the full model, create Data Transfer Objects:

```csharp
public record CreateMaintenanceRequest(
    string MaintenanceTitle,
    string MaintenanceDescription,
    DateTime MaintenanceDueDate,
    string? RequiredTools = null,
    string? ToolLocation = null
);

public record MaintenanceResponse(
    string Id,
    string AssetId,
    string MaintenanceTitle,
    DateTime MaintenanceDueDate,
    string MaintenanceStatus
);
```

### 8. **Implement Soft Deletes**

Instead of hard deleting, add an `IsDeleted` flag:

```csharp
public class Maintenance
{
    // ... other fields
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}

// Query filter
var filter = Builders<Maintenance>.Filter.And(
    Builders<Maintenance>.Filter.Eq(m => m.AssetId, assetId),
    Builders<Maintenance>.Filter.Eq(m => m.IsDeleted, false)
);
```

### 9. **Add Timestamp Tracking**

```csharp
public class Maintenance
{
    // ... other fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
```

### 10. **Use Transactions for Multi-Step Operations**

```csharp
// When updating both asset and maintenance
using var session = await mongoClient.StartSessionAsync();
session.StartTransaction();

try
{
    await maintenanceService.UpdateMaintenanceAsync(maintenanceId, maintenance, session);
    await assetService.UpdateAssetStatusAsync(assetId, "available", session);
    await session.CommitTransactionAsync();
}
catch
{
    await session.AbortTransactionAsync();
    throw;
}
```

---

## Example Request/Response Flows

### Flow 1: Creating First Maintenance for a New Asset

**Step 1**: Create the asset
```http
POST /api/assets HTTP/1.1
X-User-Id: user123
Content-Type: application/json

{
  "itemName": "Refrigerator",
  "brandName": "Whirlpool",
  "category": "Appliances",
  "purchaseDate": "2024-01-15T00:00:00Z",
  "purchaseCost": 1299.99,
  "currentLocation": "Kitchen",
  "status": "available"
}
```

**Response**:
```http
HTTP/1.1 201 Created
Location: /api/assets/670abcdef1234567890abcde

{
  "id": "670abcdef1234567890abcde",
  "itemName": "Refrigerator",
  "brandName": "Whirlpool",
  "category": "Appliances",
  "ownerUserId": "user123",
  "status": "available"
}
```

**Step 2**: Add initial maintenance schedule
```http
POST /api/assets/670abcdef1234567890abcde/maintenance HTTP/1.1
Content-Type: application/json

{
  "brandName": "Whirlpool",
  "productName": "Model WRF555SDFZ",
  "purchaseLocation": "Lowes",
  "costPaid": 1299.99,
  "maintenanceDueDate": "2025-06-01T00:00:00Z",
  "maintenanceTitle": "Replace Water Filter",
  "maintenanceDescription": "Replace with EveryDrop 4 filter",
  "maintenanceStatus": "pending",
  "requiredTools": "None - hand twist removal",
  "toolLocation": "N/A"
}
```

**Response**:
```http
HTTP/1.1 201 Created
Location: /api/maintenance/671234567890abcdef123456

{
  "id": "671234567890abcdef123456",
  "assetId": "670abcdef1234567890abcde",
  "brandName": "Whirlpool",
  "productName": "Model WRF555SDFZ",
  "maintenanceTitle": "Replace Water Filter",
  "maintenanceDueDate": "2025-06-01T00:00:00Z",
  "maintenanceStatus": "pending"
}
```

---

### Flow 2: Viewing All Maintenance for an Asset

**Request**:
```http
GET /api/assets/670abcdef1234567890abcde/maintenance HTTP/1.1
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "id": "671234567890abcdef123456",
    "assetId": "670abcdef1234567890abcde",
    "maintenanceTitle": "Replace Water Filter",
    "maintenanceDueDate": "2025-06-01T00:00:00Z",
    "maintenanceStatus": "pending"
  },
  {
    "id": "671234567890abcdef123457",
    "assetId": "670abcdef1234567890abcde",
    "maintenanceTitle": "Clean Coils",
    "maintenanceDueDate": "2026-01-15T00:00:00Z",
    "maintenanceStatus": "pending"
  }
]
```

---

### Flow 3: Dashboard View (All Maintenance)

**Request**:
```http
GET /api/maintenace HTTP/1.1
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "id": "671234567890abcdef123456",
    "assetId": "670abcdef1234567890abcde",
    "brandName": "Whirlpool",
    "maintenanceTitle": "Replace Water Filter",
    "maintenanceDueDate": "2025-06-01T00:00:00Z",
    "maintenanceStatus": "pending"
  },
  {
    "id": "671234567890abcdef123458",
    "assetId": "670abcdef1234567890abcdf",
    "brandName": "Craftsman",
    "maintenanceTitle": "Oil Change",
    "maintenanceDueDate": "2025-05-01T00:00:00Z",
    "maintenanceStatus": "pending"
  },
  {
    "id": "671234567890abcdef123459",
    "assetId": "670abcdef1234567890abce0",
    "brandName": "GE",
    "maintenanceTitle": "Filter Cleaning",
    "maintenanceDueDate": "2025-04-15T00:00:00Z",
    "maintenanceStatus": "completed"
  }
]
```

---

### Flow 4: Completing Maintenance (Requires New Endpoint)

**Assuming UPDATE endpoint is added as recommended**:

```http
PUT /api/maintenance/671234567890abcdef123456 HTTP/1.1
Content-Type: application/json

{
  "assetId": "670abcdef1234567890abcde",
  "brandName": "Whirlpool",
  "productName": "Model WRF555SDFZ",
  "maintenanceTitle": "Replace Water Filter",
  "maintenanceDueDate": "2025-06-01T00:00:00Z",
  "maintenanceStatus": "completed",
  "maintenanceDescription": "Filter replaced successfully on 2025-06-01",
  "preserveFromPrior": true
}
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": "671234567890abcdef123456",
  "assetId": "670abcdef1234567890abcde",
  "maintenanceStatus": "completed",
  "preserveFromPrior": true
}
```

---

## Conclusion

The maintenance endpoints in HippoExchange provide a foundation for tracking asset maintenance, but several improvements are recommended:

### Critical Issues to Address:
1. Fix the typo in `/api/maintenace` → `/api/maintenance`
2. Add missing UPDATE and DELETE endpoints
3. Implement authentication/authorization on all maintenance endpoints
4. Add asset existence validation before creating maintenance

### Recommended Enhancements:
1. Pagination for large result sets
2. Filtering and sorting capabilities
3. Input validation
4. Cascade deletion handling
5. Time zone handling improvements

### Security Priorities:
1. User ownership validation
2. Filter global maintenance by user
3. Validate ObjectIds to prevent exceptions

By addressing these issues, the maintenance endpoints will be more robust, secure, and user-friendly.
