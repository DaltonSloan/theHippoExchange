# Testing Guide: Unsafe Metadata & User Profile Updates

## Answer to Your Question: Test Reliability

**Should you make something external to test before trying from the front end?**

**YES, I strongly recommend testing externally first.** Here's why:

### Why External Testing is Better:

1. **Faster Iteration**: You can quickly test and debug without front-end complications
2. **Isolated Testing**: Confirms the backend logic works independently
3. **Clear Error Messages**: You'll see exact error responses without UI interference
4. **Controlled Environment**: You can test edge cases easily
5. **Documentation**: Creates reusable test cases for the team

### Testing Reliability Analysis:

| Test Method | Reliability | Speed | Debugging | Recommendation |
|-------------|-------------|-------|-----------|----------------|
| **Database Seeding** | ⭐⭐⭐ Moderate | Fast | Hard to debug | Good for initial data setup |
| **Postman/cURL** | ⭐⭐⭐⭐⭐ Excellent | Fast | Easy | **BEST - Start here** |
| **Automated Tests** | ⭐⭐⭐⭐⭐ Excellent | Medium | Easy | Best for CI/CD |
| **Front-end Testing** | ⭐⭐ Low | Slow | Very Hard | Do this LAST |

---

## 🧪 Recommended Testing Approach

### Phase 1: Test with Seeded Data (5 minutes)
Verify that seeded users have phone numbers and addresses.

### Phase 2: Test Webhook Handler (15 minutes)
Test the `user.created` webhook endpoint with unsafe_metadata.

### Phase 3: Test PATCH Endpoint (15 minutes)
Test updating user profiles with nested address objects.

### Phase 4: Front-end Integration (Only after Phases 1-3 pass)

---

## Phase 1: Verify Seeded Data

### Step 1.1: Seed the Database
```bash
cd /workspace
dotnet run --project src/HippoExchange.Api/ seed
```

### Step 1.2: Check Seeded User Data
```bash
# Get John Smith's profile (PROD)
curl -X GET http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT

# Or for DEV environment:
curl -X GET http://localhost:8080/users/user_33fKj66bKWI3f60HIg0L1tuUvip
```

### Expected Response:
```json
{
  "clerk_id": "user_33UeIDzYloCoZABaaCR1WPmV7MT",
  "email": "john.smith@demo.hippoexchange.com",
  "username": "john_smith",
  "first_name": "John",
  "last_name": "Smith",
  "phone_number": "+1-555-0101",
  "address": {
    "street": "123 Maple Street",
    "city": "Springfield",
    "state": "IL",
    "postal_code": "62701",
    "country": "USA"
  }
}
```

✅ **Pass Criteria**: User has `phone_number` and `address` fields populated.

---

## Phase 2: Test Webhook Handler (user.created)

### Step 2.1: Test with Complete unsafe_metadata

**Using cURL:**
```bash
curl -X POST http://localhost:8080/api/webhooks/clerk \
  -H "Content-Type: application/json" \
  -d '{
    "type": "user.created",
    "data": {
      "id": "user_test_webhook_001",
      "object": "user",
      "username": "test_webhook_user",
      "first_name": "Webhook",
      "last_name": "Testuser",
      "image_url": "https://example.com/image.png",
      "has_image": false,
      "primary_email_address_id": "idn_test_001",
      "password_enabled": true,
      "two_factor_enabled": false,
      "email_addresses": [
        {
          "id": "idn_test_001",
          "email_address": "webhook.test@example.com",
          "verification": {
            "status": "verified",
            "strategy": "email_code"
          }
        }
      ],
      "unsafe_metadata": {
        "phoneNumber": "+1-555-9999",
        "address": {
          "street": "999 Webhook Street",
          "city": "Test City",
          "state": "CA",
          "postal_code": "90210",
          "country": "USA"
        }
      },
      "last_sign_in_at": 1678886400000,
      "banned": false,
      "created_at": 1726942676273,
      "updated_at": 1678886400000
    },
    "object": "event",
    "timestamp": 1678886400000
  }'
```

**Expected Response:**
```json
{
  "message": "User created or updated successfully"
}
```

### Step 2.2: Verify User Was Created with unsafe_metadata
```bash
curl -X GET http://localhost:8080/users/user_test_webhook_001
```

**Expected Response:**
```json
{
  "clerk_id": "user_test_webhook_001",
  "email": "webhook.test@example.com",
  "username": "test_webhook_user",
  "first_name": "Webhook",
  "last_name": "Testuser",
  "phone_number": "+1-555-9999",
  "address": {
    "street": "999 Webhook Street",
    "city": "Test City",
    "state": "CA",
    "postal_code": "90210",
    "country": "USA"
  }
}
```

✅ **Pass Criteria**: 
- Phone number from unsafe_metadata is saved
- Address from unsafe_metadata is saved with all nested fields

### Step 2.3: Test with Missing unsafe_metadata (Edge Case)

```bash
curl -X POST http://localhost:8080/api/webhooks/clerk \
  -H "Content-Type: application/json" \
  -d '{
    "type": "user.created",
    "data": {
      "id": "user_test_no_metadata",
      "username": "no_metadata_user",
      "first_name": "No",
      "last_name": "Metadata",
      "email_addresses": [
        {
          "id": "idn_test_002",
          "email_address": "no.metadata@example.com"
        }
      ]
    }
  }'
```

✅ **Pass Criteria**: User is created without errors, phone_number and address are null.

### Step 2.4: Test with Partial unsafe_metadata (Edge Case)

```bash
curl -X POST http://localhost:8080/api/webhooks/clerk \
  -H "Content-Type: application/json" \
  -d '{
    "type": "user.created",
    "data": {
      "id": "user_test_partial_metadata",
      "username": "partial_user",
      "first_name": "Partial",
      "last_name": "Data",
      "email_addresses": [
        {
          "id": "idn_test_003",
          "email_address": "partial@example.com"
        }
      ],
      "unsafe_metadata": {
        "phoneNumber": "+1-555-7777"
      }
    }
  }'
```

✅ **Pass Criteria**: User has phone_number but address is null.

---

## Phase 3: Test PATCH /users/{userId} Endpoint

### Prerequisites:
You need a valid Clerk JWT token. For testing, you can use the seeded demo users.

### Step 3.1: Update Both Phone and Address

```bash
# Get a Clerk JWT token first by logging into your Clerk account
# Replace YOUR_CLERK_JWT_TOKEN with actual token

curl -X PATCH http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT \
  -H "Authorization: Bearer YOUR_CLERK_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+1-555-1234",
    "address": {
      "street": "100 Updated Street",
      "city": "New City",
      "state": "NY",
      "postal_code": "10001",
      "country": "USA"
    }
  }'
```

**Expected Response:**
```json
{
  "message": "Profile updated successfully."
}
```

### Step 3.2: Verify Update
```bash
curl -X GET http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT
```

✅ **Pass Criteria**: Phone number and address are updated to new values.

### Step 3.3: Update Only Address (Partial Update)

```bash
curl -X PATCH http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT \
  -H "Authorization: Bearer YOUR_CLERK_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "address": {
      "street": "200 Another Street",
      "city": "Chicago",
      "state": "IL",
      "postal_code": "60601",
      "country": "USA"
    }
  }'
```

✅ **Pass Criteria**: Only address is updated, phone number remains unchanged.

### Step 3.4: Update Only Phone Number

```bash
curl -X PATCH http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT \
  -H "Authorization: Bearer YOUR_CLERK_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+1-555-9876"
  }'
```

✅ **Pass Criteria**: Only phone number is updated, address remains unchanged.

### Step 3.5: Test Validation (Should Fail)

```bash
# Test with overly long street name (>100 characters)
curl -X PATCH http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT \
  -H "Authorization: Bearer YOUR_CLERK_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "address": {
      "street": "This is an extremely long street name that exceeds the maximum allowed length of one hundred characters for testing validation",
      "city": "Test City",
      "state": "CA"
    }
  }'
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": [
    "Max length is 100 character and a minimum of 0"
  ]
}
```

✅ **Pass Criteria**: Request is rejected with validation error.

### Step 3.6: Test Authorization (Should Fail)

```bash
# Try to update another user's profile
curl -X PATCH http://localhost:8080/users/user_33UeKv6eNbmLb2HClHd1PN51AZ5 \
  -H "Authorization: Bearer YOUR_CLERK_JWT_TOKEN_FOR_JOHN_SMITH" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+1-555-0000"
  }'
```

**Expected Response (401 Unauthorized):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

✅ **Pass Criteria**: Request is rejected as unauthorized.

---

## 📋 Testing Checklist

### Webhook Handler (user.created)
- [ ] ✅ User created with complete unsafe_metadata (phone + address)
- [ ] ✅ User created without unsafe_metadata (graceful handling)
- [ ] ✅ User created with partial unsafe_metadata (phone only)
- [ ] ✅ Address fields are correctly parsed (street, city, state, postal_code, country)
- [ ] ✅ Invalid JSON in unsafe_metadata doesn't crash the server

### PATCH Endpoint
- [ ] ✅ Update both phone and address
- [ ] ✅ Update only phone number
- [ ] ✅ Update only address (nested object)
- [ ] ✅ Partial address update (some fields omitted)
- [ ] ✅ Validation errors are returned correctly
- [ ] ✅ Authorization checks prevent unauthorized updates
- [ ] ✅ Input sanitization prevents XSS attacks

### Database Verification
- [ ] ✅ Seeded users have phone numbers
- [ ] ✅ Seeded users have addresses
- [ ] ✅ Address is stored as nested object in MongoDB
- [ ] ✅ Updates are reflected in database immediately

---

## 🔧 Debugging Tips

### If Webhook Fails:
1. Check server logs for error messages
2. Verify JSON structure matches `ClerkUserData` model
3. Test with Swagger UI at `http://localhost:8080/swagger`

### If PATCH Fails:
1. Verify Clerk JWT token is valid
2. Check that you're updating your own user profile
3. Verify address validation constraints

### Common Issues:
- **401 Unauthorized**: Get a fresh Clerk JWT token
- **400 Bad Request**: Check JSON structure and validation rules
- **404 Not Found**: Verify user exists in database

---

## 🚀 Quick Test Script

Save this as `test_unsafe_metadata.sh`:

```bash
#!/bin/bash

echo "=== Testing Unsafe Metadata Implementation ==="

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Test 1: Seed Database
echo -e "\n📦 Test 1: Seeding database..."
cd /workspace && dotnet run --project src/HippoExchange.Api/ seed
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Database seeded successfully${NC}"
else
    echo -e "${RED}❌ Database seeding failed${NC}"
    exit 1
fi

# Test 2: Verify Seeded User
echo -e "\n👤 Test 2: Checking seeded user data..."
response=$(curl -s http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT)
if echo "$response" | grep -q "phone_number"; then
    echo -e "${GREEN}✅ Seeded user has phone number${NC}"
else
    echo -e "${RED}❌ Seeded user missing phone number${NC}"
fi

if echo "$response" | grep -q "address"; then
    echo -e "${GREEN}✅ Seeded user has address${NC}"
else
    echo -e "${RED}❌ Seeded user missing address${NC}"
fi

# Test 3: Webhook with unsafe_metadata
echo -e "\n🪝 Test 3: Testing webhook with unsafe_metadata..."
webhook_response=$(curl -s -X POST http://localhost:8080/api/webhooks/clerk \
  -H "Content-Type: application/json" \
  -d '{
    "type": "user.created",
    "data": {
      "id": "user_test_script_001",
      "username": "script_test",
      "first_name": "Script",
      "last_name": "Test",
      "email_addresses": [{
        "id": "idn_test",
        "email_address": "script@test.com"
      }],
      "unsafe_metadata": {
        "phoneNumber": "+1-555-TEST",
        "address": {
          "street": "123 Test St",
          "city": "Test City",
          "state": "TS",
          "postal_code": "12345",
          "country": "USA"
        }
      }
    }
  }')

if echo "$webhook_response" | grep -q "successfully"; then
    echo -e "${GREEN}✅ Webhook processed successfully${NC}"
else
    echo -e "${RED}❌ Webhook failed${NC}"
fi

# Test 4: Verify webhook created user correctly
echo -e "\n🔍 Test 4: Verifying webhook created user..."
user_response=$(curl -s http://localhost:8080/users/user_test_script_001)
if echo "$user_response" | grep -q "555-TEST"; then
    echo -e "${GREEN}✅ Phone number from unsafe_metadata saved${NC}"
else
    echo -e "${RED}❌ Phone number not saved${NC}"
fi

if echo "$user_response" | grep -q "Test City"; then
    echo -e "${GREEN}✅ Address from unsafe_metadata saved${NC}"
else
    echo -e "${RED}❌ Address not saved${NC}"
fi

echo -e "\n🎉 Testing complete!"
```

Make it executable and run:
```bash
chmod +x test_unsafe_metadata.sh
./test_unsafe_metadata.sh
```

---

## ✅ Recommendation Summary

**Start with this order:**

1. **Seed Database** (2 min) - Verify seeded data has phone/address
2. **Test Webhooks with cURL** (10 min) - Test all unsafe_metadata scenarios
3. **Test PATCH with cURL** (10 min) - Test all update scenarios
4. **Create Postman Collection** (5 min) - Save test cases for reuse
5. **Front-end Integration** (30+ min) - Only after backend is proven to work

**This approach will save you hours of debugging and give you confidence that your backend implementation is solid before involving the front-end.**
