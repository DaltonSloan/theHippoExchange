# Demo User Credentials - Quick Reference

> **Authentication update:** API requests now require a Clerk-issued bearer token in the `Authorization` header. Replace any `X-User-Id` examples below with `Authorization: Bearer <token>` when calling the API directly.

## Real Clerk User IDs

These are the actual Clerk user IDs created in your Clerk dashboard. Use these for testing with the seeded data.

### John Smith (Homeowner)
- **Clerk ID:** `user_33UeIDzYloCoZABaaCR1WPmV7MT`
- **Username:** `john_smith`
- **Email:** `john.smith@demo.hippoexchange.com`
- **Email ID:** `idn_33UeI8ZFWT796TFQuscbvSCXayJ`
- **Persona:** Homeowner with lawn & garden equipment
- **Assets:** 7 items (lawn mower, leaf blower, pressure washer, etc.)
- **Location:** Springfield, IL

### Jane Doe (Hobbyist)
- **Clerk ID:** `user_33UeKv6eNbmLb2HClHd1PN51AZ5`
- **Username:** `jane_doe`
- **Email:** `jane.doe@demo.hippoexchange.com`
- **Email ID:** `idn_33UeKuwQnPVVaByJV4qZu4DXnuQ`
- **Persona:** Hobbyist with workshop tools
- **Assets:** 9 items (table saw, drill, miter saw, etc.)
- **Location:** Portland, OR

### Bob Builder (Contractor)
- **Clerk ID:** `user_33UeOCZ7LGxjHJ8dkwnAIozslO0`
- **Username:** `bob_builder`
- **Email:** `bob.builder@demo.hippoexchange.com`
- **Email ID:** `idn_33UeOAgYquzo8uNfv9risd0VOmO`
- **Persona:** Professional contractor with heavy equipment
- **Assets:** 10 items (nailer, impact driver, tile saw, etc.)
- **Location:** Austin, TX

---

## Usage Examples

### API Testing with curl

```bash
# Get John Smith's assets
curl -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT" \
  http://localhost:8080/api/assets

# Get Jane Doe's assets
curl -H "X-User-Id: user_33UeKv6eNbmLb2HClHd1PN51AZ5" \
  http://localhost:8080/api/assets

# Get Bob Builder's assets
curl -H "X-User-Id: user_33UeOCZ7LGxjHJ8dkwnAIozslO0" \
  http://localhost:8080/api/assets

# Create a new asset for John Smith
curl -X POST http://localhost:8080/api/assets \
  -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT" \
  -H "Content-Type: application/json" \
  -d '{
    "itemName": "Garden Rake",
    "brandName": "Fiskars",
    "category": "Lawn & Garden",
    "purchaseDate": "2024-06-15T00:00:00Z",
    "purchaseCost": 29.99,
    "currentLocation": "Shed",
    "conditionDescription": "Brand new",
    "status": "available",
    "favorite": false
  }'
```

### Swagger UI

1. Open `http://localhost:8080/swagger`
2. Click "Authorize" button
3. Enter one of the Clerk IDs in the `X-User-Id` field:
   - `user_33UeIDzYloCoZABaaCR1WPmV7MT` (John Smith)
   - `user_33UeKv6eNbmLb2HClHd1PN51AZ5` (Jane Doe)
   - `user_33UeOCZ7LGxjHJ8dkwnAIozslO0` (Bob Builder)
4. Test any endpoint

### Seeding Commands

```bash
# Seed database with demo data
cd /workspace/src/HippoExchange.Api
dotnet run seed

# Or via API
curl -X POST http://localhost:8080/api/admin/seed

# Check status
curl http://localhost:8080/api/admin/seed/status
```

---

## Important Notes

✅ **These are real Clerk users** - They were created in your Clerk dashboard  
✅ **Emails are verified** - All users have verified email addresses  
✅ **Can authenticate** - These users can actually log in via Clerk  
✅ **Data persists** - Seeded assets and maintenance are linked to these real user IDs  

⚠️ **Do not delete these users from Clerk** - The seeding script expects these specific IDs  
⚠️ **MongoDB data** - Seeding only affects MongoDB, not Clerk user data  

---

## Profile Images

All users have Clerk-generated avatar images:

- **John Smith:** `https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVJRHpZbG9Db1pBQmFhQ1IxV1BtVjdNVCIsImluaXRpYWxzIjoiSlMifQ`

- **Jane Doe:** `https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVLdjZlTmJtTGIySENsSGQxUE41MUFaNSIsImluaXRpYWxzIjoiSkQifQ`

- **Bob Builder:** `https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVPQ1o3TEd4akhKOGRrd25BSW96c2xPMCIsImluaXRpYWxzIjoiQkIifQ`

---

## Testing Workflow

### 1. First Time Setup
```bash
# Seed the database
dotnet run seed

# Verify seeding
curl http://localhost:8080/api/admin/seed/status
```

### 2. Test with Each User
```bash
# Test John Smith's account
curl -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT" \
  http://localhost:8080/api/assets | jq

# Test Jane Doe's account
curl -H "X-User-Id: user_33UeKv6eNbmLb2HClHd1PN51AZ5" \
  http://localhost:8080/api/assets | jq

# Test Bob Builder's account
curl -H "X-User-Id: user_33UeOCZ7LGxjHJ8dkwnAIozslO0" \
  http://localhost:8080/api/assets | jq
```

### 3. Test Asset Creation
```bash
# Add an asset for John
curl -X POST http://localhost:8080/api/assets \
  -H "X-User-Id: user_33UeIDzYloCoZABaaCR1WPmV7MT" \
  -H "Content-Type: application/json" \
  -d '{"itemName":"Test Item","brandName":"Test","category":"Other","purchaseDate":"2024-01-01T00:00:00Z","purchaseCost":100,"currentLocation":"Home","status":"available"}'
```

### 4. View in MongoDB
1. Open Mongo Express: `http://localhost:8081` (admin/admin)
2. Navigate to `hippo-exchange` database
3. View `users`, `assets`, and `maintenance` collections
4. Verify data is linked correctly via Clerk IDs

---

## Copy-Paste Snippets

Quick copy-paste for common tasks:

**John Smith's Clerk ID:**
```
user_33UeIDzYloCoZABaaCR1WPmV7MT
```

**Jane Doe's Clerk ID:**
```
user_33UeKv6eNbmLb2HClHd1PN51AZ5
```

**Bob Builder's Clerk ID:**
```
user_33UeOCZ7LGxjHJ8dkwnAIozslO0
```

**All three for scripts:**
```bash
JOHN_SMITH="user_33UeIDzYloCoZABaaCR1WPmV7MT"
JANE_DOE="user_33UeKv6eNbmLb2HClHd1PN51AZ5"
BOB_BUILDER="user_33UeOCZ7LGxjHJ8dkwnAIozslO0"
```
