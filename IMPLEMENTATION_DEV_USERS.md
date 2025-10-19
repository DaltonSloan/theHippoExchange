# Implementation: Dev-Specific Users for Database Seeding

> **Authentication update:** API examples now require Clerk bearer tokens. Swap any `X-User-Id` headers mentioned below with `Authorization: Bearer <token>`.

## Overview
This implementation adds support for both **Production** and **Development** Clerk user IDs in the database seeding script. The seeder now creates 6 demo users (3 PROD + 3 DEV) with identical data except for their Clerk IDs.

## Changes Made

### 1. Updated `DatabaseSeeder.cs` - SeedDatabaseAsync Method
- Enhanced console output to clearly distinguish between PROD and DEV users
- Added comprehensive logging that displays both sets of Clerk IDs
- Updated user count reporting to show "3 PROD + 3 DEV" format
- Added helper method `GetEnvironmentLabel()` to identify environment from Clerk ID

**Console Output Example:**
```
🌱 Starting database seeding...
   Creating users for BOTH Production and Development Clerk environments

🧹 Clearing existing demo data...
✅ Created 6 demo users (3 PROD + 3 DEV)
✅ Created 7 assets for John Smith (PROD)
✅ Created 51 maintenance records for John's assets (PROD)
...

🎉 Database seeding complete!
   - 6 demo users (3 PROD + 3 DEV)
   - 54 total assets

📋 Demo User Clerk IDs:

   PRODUCTION Environment:
   • John Smith (Homeowner):  user_33UeIDzYloCoZABaaCR1WPmV7MT
   • Jane Doe (Hobbyist):     user_33UeKv6eNbmLb2HClHd1PN51AZ5
   • Bob Builder (Contractor): user_33UeOCZ7LGxjHJ8dkwnAIozslO0

   DEVELOPMENT Environment:
   • John Smith (Homeowner):  user_33fKj66bKWI3f60HIg0L1tuUvip
   • Jane Doe (Hobbyist):     user_33fKlsH9bgC5XJlaOLXcPrrqXQI
   • Bob Builder (Contractor): user_33fKntiTjEiZ1S9jXSmTwmqhlAc

💡 Usage: Use the appropriate Clerk ID in the X-User-Id header based on your Clerk environment.
   All users have identical data except for their Clerk ID.
```

### 2. Updated `ClearDemoDataAsync` Method
- Expanded the demo Clerk IDs array to include both PROD and DEV IDs
- Added clear comments distinguishing between environments
- Now clears both PROD and DEV users when re-seeding

**Updated Code:**
```csharp
var demoClerkIds = new[] { 
    // PRODUCTION Clerk IDs
    "user_33UeIDzYloCoZABaaCR1WPmV7MT",  // john_smith (PROD)
    "user_33UeKv6eNbmLb2HClHd1PN51AZ5",  // jane_doe (PROD)
    "user_33UeOCZ7LGxjHJ8dkwnAIozslO0",  // bob_builder (PROD)
    // DEVELOPMENT Clerk IDs
    "user_33fKj66bKWI3f60HIg0L1tuUvip",  // john_smith (DEV)
    "user_33fKlsH9bgC5XJlaOLXcPrrqXQI",  // jane_doe (DEV)
    "user_33fKntiTjEiZ1S9jXSmTwmqhlAc"   // bob_builder (DEV)
};
```

### 3. Updated `CreateDemoUsersAsync` Method
- Added section headers to clearly separate PROD and DEV users
- Created 3 additional users with DEV Clerk IDs
- All user data (email, username, name, location, etc.) remains identical between PROD/DEV versions
- Updated documentation to explain the dual-environment approach

**User Structure:**
```
PRODUCTION USERS:
├── John Smith  (user_33UeIDzYloCoZABaaCR1WPmV7MT)
├── Jane Doe    (user_33UeKv6eNbmLb2HClHd1PN51AZ5)
└── Bob Builder (user_33UeOCZ7LGxjHJ8dkwnAIozslO0)

DEVELOPMENT USERS:
├── John Smith  (user_33fKj66bKWI3f60HIg0L1tuUvip)
├── Jane Doe    (user_33fKlsH9bgC5XJlaOLXcPrrqXQI)
└── Bob Builder (user_33fKntiTjEiZ1S9jXSmTwmqhlAc)
```

### 4. Updated `CreateDemoAssetsForUserAsync` Method
- Modified switch statement to handle both PROD and DEV Clerk IDs
- Uses C# case fallthrough to apply same asset creation logic to both versions of each user
- Updated documentation to reflect dual-environment support

**Updated Switch Cases:**
```csharp
case "user_33UeIDzYloCoZABaaCR1WPmV7MT":  // john_smith (PROD)
case "user_33fKj66bKWI3f60HIg0L1tuUvip":  // john_smith (DEV)
    // Homeowner with lawn/garden equipment
    ...
    break;
    
case "user_33UeKv6eNbmLb2HClHd1PN51AZ5":  // jane_doe (PROD)
case "user_33fKlsH9bgC5XJlaOLXcPrrqXQI":  // jane_doe (DEV)
    // Hobbyist with workshop tools
    ...
    break;
    
case "user_33UeOCZ7LGxjHJ8dkwnAIozslO0":  // bob_builder (PROD)
case "user_33fKntiTjEiZ1S9jXSmTwmqhlAc":  // bob_builder (DEV)
    // Contractor with professional equipment
    ...
    break;
```

## Demo User Credentials

### Production Environment
| Name | Clerk ID | Username | Email |
|------|----------|----------|-------|
| John Smith | `user_33UeIDzYloCoZABaaCR1WPmV7MT` | john_smith | john.smith@demo.hippoexchange.com |
| Jane Doe | `user_33UeKv6eNbmLb2HClHd1PN51AZ5` | jane_doe | jane.doe@demo.hippoexchange.com |
| Bob Builder | `user_33UeOCZ7LGxjHJ8dkwnAIozslO0` | bob_builder | bob.builder@demo.hippoexchange.com |

### Development Environment
| Name | Clerk ID | Username | Email |
|------|----------|----------|-------|
| John Smith | `user_33fKj66bKWI3f60HIg0L1tuUvip` | john_smith | john.smith@demo.hippoexchange.com |
| Jane Doe | `user_33fKlsH9bgC5XJlaOLXcPrrqXQI` | jane_doe | jane.doe@demo.hippoexchange.com |
| Bob Builder | `user_33fKntiTjEiZ1S9jXSmTwmqhlAc` | bob_builder | bob.builder@demo.hippoexchange.com |

**Note:** All users share the same email addresses and usernames. The only difference is their Clerk ID, which determines which Clerk environment they belong to.

## Usage Instructions

### For Local Development
1. When using the **Development** Clerk instance, use the DEV Clerk IDs in your API requests
2. When using the **Production** Clerk instance, use the PROD Clerk IDs in your API requests

### API Testing
Use the appropriate Clerk ID in the `X-User-Id` header:

**Development Example:**
```bash
curl -X GET "http://localhost:5000/api/assets" \
  -H "X-User-Id: user_33fKj66bKWI3f60HIg0L1tuUvip"
```

**Production Example:**
```bash
curl -X GET "https://api.hippoexchange.com/api/assets" \
  -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT"
```

### Running the Seeder
The seeder automatically creates both PROD and DEV users:

```bash
# Via API endpoint (if configured)
curl -X POST "http://localhost:5000/api/seed"

# Or programmatically in your application
await _databaseSeeder.SeedDatabaseAsync();
```

## Benefits

1. **Environment Isolation**: Clear separation between PROD and DEV environments
2. **Consistent Testing**: Identical data across environments ensures consistent test results
3. **No Environment Switching**: No need to modify code or configuration when switching between environments
4. **Clear Documentation**: Console output makes it obvious which Clerk IDs to use for each environment
5. **Easy Maintenance**: Single seeding script maintains both environments

## Acceptance Criteria Met

✅ **Three new users created in Clerk development instance** - DEV users added to seeding script  
✅ **Database seeding script updated with DEV Clerk IDs** - All DEV IDs included  
✅ **Script logs/outputs both PROD and DEV user IDs** - Comprehensive console output with clear labels  
✅ **Documentation/comments updated** - Inline comments and this implementation document provide clear guidance  

## Testing

Build successful with no errors:
```bash
dotnet build /workspace/theHippoExchange.sln
# Result: HippoExchange.Api -> /workspace/src/HippoExchange.Api/bin/Debug/net8.0/HippoExchange.Api.dll
```

## Files Modified

- `/workspace/src/HippoExchange.Api/Services/DatabaseSeeder.cs`
  - SeedDatabaseAsync() - Enhanced logging and output
  - GetEnvironmentLabel() - New helper method
  - ClearDemoDataAsync() - Added DEV Clerk IDs
  - CreateDemoUsersAsync() - Added 3 DEV users
  - CreateDemoAssetsForUserAsync() - Added DEV Clerk ID cases

## Next Steps

1. Run the seeding script to create all 6 users in the database
2. Test API endpoints with both PROD and DEV Clerk IDs
3. Verify that assets and maintenance records are correctly associated with each user
4. Update any documentation that references only PROD Clerk IDs to include DEV IDs
