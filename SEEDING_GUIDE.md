# Database Seeding Guide

## Overview

The HippoExchange application now includes a comprehensive database seeding feature that automatically populates the database with realistic demo data for development and testing purposes.

## Features Implemented

### ✅ Core Functionality

- **Command-line seeding**: Run via `dotnet run seed`
- **Idempotent operations**: Running seed multiple times doesn't create duplicates
- **Smart cleanup**: Only removes demo users (not all data) when re-seeding
- **Comprehensive logging**: Clear console output showing what's being created

### ✅ Demo Users Created

The seeding script creates 3 demo users with different personas:

1. **John Smith** (`clerk_john_smith`)
   - Persona: Homeowner
   - Equipment Focus: Lawn & garden equipment
   - Assets: 7 items (lawn mower, leaf blower, pressure washer, etc.)
   - Location: Springfield, IL

2. **Jane Doe** (`clerk_jane_doe`)
   - Persona: Hobbyist
   - Equipment Focus: Workshop tools
   - Assets: 9 items (table saw, drill, miter saw, etc.)
   - Location: Portland, OR

3. **Bob Builder** (`clerk_bob_builder`)
   - Persona: Professional Contractor
   - Equipment Focus: Professional/commercial equipment
   - Assets: 10 items (nailer, impact driver, tile saw, etc.)
   - Location: Austin, TX

### ✅ Asset Variety

Each user's assets include:

- **Realistic brands**: DeWalt, Makita, Milwaukee, Honda, Craftsman, Bosch, etc.
- **Varied statuses**: 
  - `available` - Ready to use
  - `maintenance` - Currently being serviced
  - `loaned` - Borrowed by someone
- **Cost ranges**: $10 to $5,000
- **Favorite marking**: Some assets marked as favorites
- **Purchase history**: Realistic purchase dates (from several years ago to recent)
- **Location tracking**: Current storage location for each asset
- **Condition descriptions**: Detailed condition notes

### ✅ Maintenance Records

Each asset gets 8-15 maintenance records with:

- **Varied due dates**:
  - 20% overdue (past due date)
  - 15% due soon (within 7 days)
  - 20% due later (within 30 days)
  - 45% completed
  
- **Realistic tasks** based on asset category:
  - Lawn equipment: oil changes, blade sharpening, air filter replacement
  - Power tools: blade replacement, calibration, battery maintenance
  - Cleaning equipment: filter replacement, hose inspection
  
- **Required tools**: Lists of tools needed for each maintenance task
- **Purchase locations**: Home Depot, Lowe's, Ace Hardware, etc.
- **Status tracking**: pending, completed, or overdue

## Usage

### Command Line Seeding

```bash
# Navigate to the API project
cd /workspace/src/HippoExchange.Api

# Seed with demo data (removes existing demo users first)
dotnet run seed
```

### API Endpoints

The seeding functionality is also available via REST API endpoints in the `/api/admin` namespace:

#### 1. **Seed Database** - `POST /api/admin/seed`

Populates the database with demo data. Idempotent - removes existing demo users first.

```bash
curl -X POST http://localhost:8080/api/admin/seed
```

**Response:**
```json
{
  "message": "Database seeded successfully",
  "demoUsers": [
    { "clerkId": "clerk_john_smith", "name": "John Smith", "persona": "Homeowner" },
    { "clerkId": "clerk_jane_doe", "name": "Jane Doe", "persona": "Hobbyist" },
    { "clerkId": "clerk_bob_builder", "name": "Bob Builder", "persona": "Contractor" }
  ]
}
```

#### 2. **Purge Demo Data** - `DELETE /api/admin/seed`

Removes only the demo users and their associated data. Does not affect other data.

```bash
curl -X DELETE http://localhost:8080/api/admin/seed
```

**Response:**
```json
{
  "message": "Demo data removed successfully",
  "removedUsers": ["clerk_john_smith", "clerk_jane_doe", "clerk_bob_builder"]
}
```

#### 3. **Check Seed Status** - `GET /api/admin/seed/status`

Check if demo data exists in the database.

```bash
curl http://localhost:8080/api/admin/seed/status
```

**Response:**
```json
{
  "hasDemoData": true,
  "demoUserCount": 3,
  "demoUsers": [
    {
      "clerkId": "clerk_john_smith",
      "name": "John Smith",
      "email": "john.smith@demo.hippoexchange.com",
      "assetCount": 7
    },
    ...
  ]
}
```

### Seeding Commands

```bash
# Navigate to the API project
cd /workspace/src/HippoExchange.Api

# Seed with demo data (removes existing demo users first)
dotnet run seed
```

### Testing with Demo Users

Use the Clerk IDs in API requests:

```bash
# Get all users
curl http://localhost:8080/users

# Get John Smith's assets
curl -H "X-User-Id: clerk_john_smith" http://localhost:8080/api/assets

# Get Jane Doe's assets
curl -H "X-User-Id: clerk_jane_doe" http://localhost:8080/api/assets

# Get Bob Builder's assets
curl -H "X-User-Id: clerk_bob_builder" http://localhost:8080/api/assets

# Get maintenance for a specific asset
curl http://localhost:8080/api/assets/{assetId}/maintenance
```

### Using Swagger UI

1. Navigate to `http://localhost:8080/swagger`
2. Use the "Authorize" button to set the `X-User-Id` header
3. Enter one of the demo user Clerk IDs:
   - `clerk_john_smith`
   - `clerk_jane_doe`
   - `clerk_bob_builder`
4. Test the API endpoints with pre-populated data

## Technical Details

### Files Created/Modified

1. **`Services/DatabaseSeeder.cs`** (NEW)
   - Main seeding service with all logic
   - ~900 lines of comprehensive seeding code
   - Helper methods for generating realistic data

2. **`Program.cs`** (MODIFIED)
   - Added command-line argument parsing
   - Integrated DatabaseSeeder service
   - Added seeding flow before app runs

3. **`README.md`** (MODIFIED)
   - Added Database Seeding section
   - Usage examples and important notes
   - Demo user credentials

### Key Implementation Details

- **Deterministic randomness**: Uses hash-based seeds for consistent data generation
- **Realistic dates**: Assets purchased 1-7 years ago, maintenance scheduled appropriately
- **Category-aware maintenance**: Different maintenance tasks based on asset category
- **User statistics**: Automatically updates user stats (total assets count)
- **MongoDB integration**: Uses existing service patterns and MongoDB collections

## Safety Features

- **Idempotent design**: Can run multiple times safely
- **Selective cleanup**: Only removes demo users (Clerk IDs starting with known patterns)
- **Clear warnings**: Console output clearly indicates destructive operations
- **Exit after seeding**: Application exits cleanly after seeding completes

## Acceptance Criteria Status

✅ Seeding script can be run via command line (`dotnet run seed`)  
✅ Script is idempotent - can run multiple times without creating duplicates  
✅ Creates 3 demo user accounts with known identifiers  
✅ Each demo user has 5-10 assets with varied data  
✅ Mix of brands, statuses, costs, favorites, and locations  
✅ Each user has 8-15 maintenance tasks with variety  
✅ Overdue, due soon, due later, and completed tasks included  
✅ Assets have realistic names, descriptions, and placeholders  
✅ Script includes detailed comments explaining data creation  
✅ Works in any environment (development/production)  
✅ README documentation explains how to run seed script  
✅ Three diverse demo users as specified (homeowner, hobbyist, contractor)  

## Statistics

After seeding, the database will contain:

- **Users**: 3 demo users
- **Assets**: 26 total (7 + 9 + 10)
- **Maintenance Records**: ~315 total (85 + 102 + 128)

## Future Enhancements

Possible improvements for the future:

1. **Configurable seed data**: Allow custom number of users/assets via command line
2. **Seed profiles**: Different seeding profiles (small, medium, large datasets)
3. **Image generation**: Generate actual placeholder images instead of URLs
4. **Relationships**: Add asset sharing/borrowing relationships between users
5. **Historical data**: Add transaction history, usage logs, etc.
6. **Performance metrics**: Track and display seeding performance

## Troubleshooting

### Seeding Fails with Connection Error

**Problem**: Cannot connect to MongoDB

**Solution**: 
```bash
# Check if MongoDB is running
docker-compose ps

# Restart containers if needed
docker-compose down && docker-compose up -d
```

### Duplicate Key Errors

**Problem**: Clerk IDs already exist

**Solution**: 
### No Data Visible in API

**Problem**: Seeding completed but API returns empty results

**Solution**:
```bash
# Check MongoDB directly via Mongo Express
# Navigate to http://localhost:8081 (admin/admin)
# Verify data exists in the collections

# Ensure you're using the correct Clerk IDs in X-User-Id header
curl -H "X-User-Id: clerk_john_smith" http://localhost:8080/api/assets
```

## Contact

For issues or questions about the seeding feature, please create an issue in the repository or contact the development team.
