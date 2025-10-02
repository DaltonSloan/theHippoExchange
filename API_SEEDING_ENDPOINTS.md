# Database Seeding API Endpoints

Quick reference for database seeding API endpoints.

## Endpoints Summary

| Method | Endpoint | Description | Destructive? |
|--------|---------### Clean Up Demo Data
```bash
# Remove only demo data
curl -X DELETE http://localhost:8080/api/admin/seed

# Verify removal
curl http://localhost:8080/api/admin/seed/status
```

## Testing with Demo Users-----------|
| `POST` | `/api/admin/seed` | Seed database with demo data | No (only removes demo users) |
| `DELETE` | `/api/admin/seed` | Remove demo data only | No (only removes demo users) |
| `GET` | `/api/admin/seed/status` | Check if demo data exists | No |

## Detailed Usage

### 1. Seed Database with Demo Data

**Endpoint:** `POST /api/admin/seed`

**Description:** Populates the database with 3 demo users, their assets, and maintenance records. This operation is idempotent - running it multiple times will not create duplicates. Existing demo users will be removed and recreated.

**Request:**
```bash
curl -X POST http://localhost:8080/api/admin/seed
```

**Response (200 OK):**
```json
{
  "message": "Database seeded successfully",
  "demoUsers": [
    {
      "clerkId": "clerk_john_smith",
      "name": "John Smith",
      "persona": "Homeowner"
    },
    {
      "clerkId": "clerk_jane_doe",
      "name": "Jane Doe",
      "persona": "Hobbyist"
    },
    {
      "clerkId": "clerk_bob_builder",
      "name": "Bob Builder",
      "persona": "Contractor"
    }
  ]
}
```

**Response (500 Error):**
```json
{
  "title": "Seeding failed",
  "status": 500,
  "detail": "Error message here"
}
```

---

### 2. Remove Demo Data Only

**Endpoint:** `DELETE /api/admin/seed`

**Description:** Removes only the demo users (`clerk_john_smith`, `clerk_jane_doe`, `clerk_bob_builder`) and their associated assets and maintenance records. Does not affect other data in the database.

**Request:**
```bash
curl -X DELETE http://localhost:8080/api/admin/seed
```

**Response (200 OK):**
```json
{
  "message": "Demo data removed successfully",
  "removedUsers": [
    "clerk_john_smith",
    "clerk_jane_doe",
    "clerk_bob_builder"
  ]
}
```

**Response (500 Error):**
```json
{
  "title": "Purge failed",
  "status": 500,
  "detail": "Error message here"
}
```

---

---

### 3. Check Seed Status

**Endpoint:** `GET /api/admin/seed/status`

**Description:** Returns information about whether demo data exists in the database and details about demo users.

**Request:**
```bash
curl http://localhost:8080/api/admin/seed/status
```

**Response (200 OK - With Demo Data):**
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
    {
      "clerkId": "clerk_jane_doe",
      "name": "Jane Doe",
      "email": "jane.doe@demo.hippoexchange.com",
      "assetCount": 9
    },
    {
      "clerkId": "clerk_bob_builder",
      "name": "Bob Builder",
      "email": "bob.builder@demo.hippoexchange.com",
      "assetCount": 10
    }
  ]
}
```

**Response (200 OK - No Demo Data):**
```json
{
  "hasDemoData": false,
  "demoUserCount": 0,
  "demoUsers": []
}
```

---

## Using with Swagger UI

All these endpoints are available in the Swagger UI under the **"Admin"** tag:

1. Navigate to `http://localhost:8080/swagger`
2. Expand the **"Admin"** section
3. Click on any endpoint to test it
4. Click "Try it out" and then "Execute"

## Workflow Examples

### First Time Setup
```bash
# Check if demo data exists
curl http://localhost:8080/api/admin/seed/status

# Seed the database
curl -X POST http://localhost:8080/api/admin/seed

# Verify seeding was successful
curl http://localhost:8080/api/admin/seed/status
```

### Re-seed with Fresh Data
```bash
# Option 1: Just re-seed (removes demo users first)
curl -X POST http://localhost:8080/api/admin/seed

# Option 2: Remove demo data, then seed again
curl -X DELETE http://localhost:8080/api/admin/seed
curl -X POST http://localhost:8080/api/admin/seed
```

### Clean Up Demo Data
```bash
# Remove only demo data
curl -X DELETE http://localhost:8080/api/admin/seed

# Verify removal
curl http://localhost:8080/api/admin/seed/status
```

### Complete Database Reset (⚠️ Dangerous)
```bash
# Reset everything and re-seed
curl -X POST http://localhost:8080/api/admin/reset

# Verify
curl http://localhost:8080/api/admin/seed/status
```

## Testing After Seeding

Once seeded, you can test the API with demo user credentials:

```bash
# Get John Smith's assets
curl -H "X-User-Id: clerk_john_smith" http://localhost:8080/api/assets

# Get Jane Doe's assets
curl -H "X-User-Id: clerk_jane_doe" http://localhost:8080/api/assets

# Get Bob Builder's assets
curl -H "X-User-Id: clerk_bob_builder" http://localhost:8080/api/assets

# Get all users
curl http://localhost:8080/users
```

## Security Notes

- These endpoints are currently **not authenticated** - they should be secured in production
- Consider adding authentication/authorization before deploying to production
- Log all seeding operations for audit purposes

## Future Enhancements

Potential improvements:

1. Add authentication/authorization to admin endpoints
2. Add confirmation parameter for destructive operations (e.g., `?confirm=true`)
3. Add ability to seed specific number of users/assets via query parameters
4. Add endpoint to seed additional users without removing existing ones
5. Add rate limiting to prevent abuse
6. Add logging and monitoring for admin operations
