# Dev User Quick Reference Guide

## Quick Copy-Paste Clerk IDs

### Development Environment (Local Testing)

**John Smith (Homeowner)**
```
user_33fKj66bKWI3f60HIg0L1tuUvip
```

**Jane Doe (Hobbyist)**
```
user_33fKlsH9bgC5XJlaOLXcPrrqXQI
```

**Bob Builder (Contractor)**
```
user_33fKntiTjEiZ1S9jXSmTwmqhlAc
```

### Production Environment

**John Smith (Homeowner)**
```
user_33UeIDzYloCoZABaaCR1WPmV7MT
```

**Jane Doe (Hobbyist)**
```
user_33UeKv6eNbmLb2HClHd1PN51AZ5
```

**Bob Builder (Contractor)**
```
user_33UeOCZ7LGxjHJ8dkwnAIozslO0
```

## API Testing Examples

### Using DEV Users (Local Development)

**Get John Smith's Assets:**
```bash
curl -X GET "http://localhost:5000/api/assets" \
  -H "X-User-Id: user_33fKj66bKWI3f60HIg0L1tuUvip"
```

**Get Jane Doe's Assets:**
```bash
curl -X GET "http://localhost:5000/api/assets" \
  -H "X-User-Id: user_33fKlsH9bgC5XJlaOLXcPrrqXQI"
```

**Get Bob Builder's Assets:**
```bash
curl -X GET "http://localhost:5000/api/assets" \
  -H "X-User-Id: user_33fKntiTjEiZ1S9jXSmTwmqhlAc"
```

### Using PROD Users (Production Testing)

**Get John Smith's Assets:**
```bash
curl -X GET "https://api.hippoexchange.com/api/assets" \
  -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT"
```

**Get Jane Doe's Assets:**
```bash
curl -X GET "https://api.hippoexchange.com/api/assets" \
  -H "X-User-Id: user_33UeKv6eNbmLb2HClHd1PN51AZ5"
```

**Get Bob Builder's Assets:**
```bash
curl -X GET "https://api.hippoexchange.com/api/assets" \
  -H "X-User-Id: user_33UeOCZ7LGxjHJ8dkwnAIozslO0"
```

## User Personas & Expected Data

### John Smith (Homeowner)
- **Profile:** Suburban homeowner with lawn and garden equipment
- **Assets:** ~7 items (lawn mower, leaf blower, hedge trimmer, pressure washer, etc.)
- **Location:** Springfield, IL
- **Use Case:** Testing typical homeowner equipment sharing scenarios

### Jane Doe (Hobbyist)
- **Profile:** DIY enthusiast with workshop tools
- **Assets:** ~9 items (table saw, drill set, miter saw, sanders, routers, etc.)
- **Location:** Portland, OR
- **Use Case:** Testing hobbyist/prosumer tool sharing scenarios

### Bob Builder (Contractor)
- **Profile:** Professional contractor with heavy-duty equipment
- **Assets:** ~10 items (framing nailer, impact driver, tile saw, concrete mixer, etc.)
- **Location:** Austin, TX
- **Use Case:** Testing professional/commercial equipment sharing scenarios

## Common Use Cases

### Testing User Profile Endpoint
```bash
# DEV - Get John Smith's profile
curl -X GET "http://localhost:5000/api/users/profile" \
  -H "X-User-Id: user_33fKj66bKWI3f60HIg0L1tuUvip"
```

### Testing Asset Creation
```bash
# DEV - Create asset for Jane Doe
curl -X POST "http://localhost:5000/api/assets" \
  -H "X-User-Id: user_33fKlsH9bgC5XJlaOLXcPrrqXQI" \
  -H "Content-Type: application/json" \
  -d '{
    "itemName": "Test Item",
    "category": "Power Tools",
    "brandName": "Test Brand"
  }'
```

### Testing Maintenance Records
```bash
# DEV - Get Bob Builder's maintenance records
curl -X GET "http://localhost:5000/api/maintenance" \
  -H "X-User-Id: user_33fKntiTjEiZ1S9jXSmTwmqhlAc"
```

## Environment Selection Guide

| Scenario | Use | Clerk IDs |
|----------|-----|-----------|
| Local development with Clerk DEV instance | DEV | `user_33fK...` |
| Staging environment with Clerk DEV instance | DEV | `user_33fK...` |
| Production environment | PROD | `user_33Ue...` |
| Testing with production Clerk data | PROD | `user_33Ue...` |

## Troubleshooting

### User Not Found Error
**Problem:** API returns "User not found"  
**Solution:** Make sure you've run the database seeder and are using the correct Clerk ID for your environment

### Wrong User Data
**Problem:** Getting different user's data than expected  
**Solution:** Verify you're using the correct Clerk ID from the appropriate environment (DEV vs PROD)

### No Assets Found
**Problem:** User exists but has no assets  
**Solution:** Re-run the database seeder to ensure assets are created for all users

## Running the Seeder

To create/refresh all demo users and their data:

```bash
# If you have a seeding endpoint
curl -X POST "http://localhost:5000/api/seed"

# Or run from the application startup if configured
# The seeder will automatically create all 6 users (3 PROD + 3 DEV)
```

## Remember

✅ All users have identical data except for Clerk IDs  
✅ Use DEV IDs (`user_33fK...`) for local development  
✅ Use PROD IDs (`user_33Ue...`) for production testing  
✅ Each user's assets and maintenance records follow their persona  
✅ The seeder is idempotent - safe to run multiple times
