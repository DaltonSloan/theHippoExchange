# PATCH /users/{userId} - Update User Profile

## Endpoint
```
PATCH /users/{userId}
```

## Description
Updates a user's profile information, including phone number and address (nested object).

## Authentication
Requires Bearer token (Clerk JWT). User can only update their own profile.

## Request Headers
```
Authorization: Bearer <clerk_jwt_token>
Content-Type: application/json
```

## Request Body
The request body accepts a JSON object with optional fields. You can update just the phone number, just the address, or both.

### Schema
```json
{
  "phoneNumber": "string (optional)",
  "address": {
    "street": "string (optional)",
    "city": "string (optional)",
    "state": "string (optional)",
    "postal_code": "string (optional)",
    "country": "string (optional)"
  }
}
```

### Example 1: Update Both Phone Number and Address
```json
{
  "phoneNumber": "+1234567890",
  "address": {
    "street": "123 Main St",
    "city": "Springfield",
    "state": "IL",
    "postal_code": "62701",
    "country": "USA"
  }
}
```

### Example 2: Update Only Phone Number
```json
{
  "phoneNumber": "+19876543210"
}
```

### Example 3: Update Only Address
```json
{
  "address": {
    "street": "456 Oak Avenue",
    "city": "Chicago",
    "state": "IL",
    "postal_code": "60601",
    "country": "USA"
  }
}
```

### Example 4: Partial Address Update
```json
{
  "address": {
    "street": "789 Pine Street",
    "city": "New York",
    "state": "NY"
  }
}
```

## Response

### Success (200 OK)
```json
{
  "message": "Profile updated successfully."
}
```

### Unauthorized (401)
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### Not Found (404)
```json
{
  "message": "User not found or profile not updated."
}
```

### Bad Request (400) - Validation Error
```json
{
  "errors": [
    "Max length is 100 character and a minimum of 0"
  ]
}
```

## Features
- ✅ **Nested Address Object**: Properly handles the address as a nested object structure
- ✅ **Input Sanitization**: All inputs are sanitized to prevent XSS and injection attacks
- ✅ **Validation**: Address fields are validated against max/min length constraints
- ✅ **Partial Updates**: Supports updating only the fields provided (phoneNumber, address, or both)
- ✅ **Authorization**: Users can only update their own profile
- ✅ **MongoDB Update**: Efficiently updates only the provided fields in the database

## cURL Examples

### Update Both Phone and Address
```bash
curl -X PATCH https://api.yourdomain.com/users/user_123456789 \
  -H "Authorization: Bearer YOUR_CLERK_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+1234567890",
    "address": {
      "street": "123 Main St",
      "city": "Springfield",
      "state": "IL",
      "postal_code": "62701",
      "country": "USA"
    }
  }'
```

### Update Only Address
```bash
curl -X PATCH https://api.yourdomain.com/users/user_123456789 \
  -H "Authorization: Bearer YOUR_CLERK_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "address": {
      "street": "456 Oak Avenue",
      "city": "Chicago",
      "state": "IL",
      "postal_code": "60601",
      "country": "USA"
    }
  }'
```

## Implementation Notes

1. **Nested Object Handling**: The endpoint correctly parses the nested `address` object from the request body using the `ProfileUpdateRequest` model with `[JsonPropertyName]` attributes.

2. **Database Logic**: The `UpdateUserProfileAsync` method in `UserService` uses MongoDB's `UpdateDefinition` builder to dynamically construct updates only for the provided fields, ensuring efficient database operations.

3. **Field Validation**: The `Address` model includes `[StringLength]` validation attributes that are enforced at the endpoint level before updating the database.

4. **Security**: Input sanitization is applied using the `InputSanitizer.SanitizeObject()` utility to prevent malicious input.
