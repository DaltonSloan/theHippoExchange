# Phone Number Format Update - Database Seeder

## Changes Made

Updated all demo user phone numbers in the database seeder to match the correct format: **+19999999999** (12 characters including the +)

### Updated Phone Numbers

| User | Environment | Old Format | New Format | Area Code |
|------|-------------|------------|------------|-----------|
| John Smith (Homeowner) | PROD & DEV | +1-555-0101 | **+12175551001** | 217 (Springfield, IL) |
| Jane Doe (Hobbyist) | PROD & DEV | +1-555-0102 | **+15035552002** | 503 (Portland, OR) |
| Bob Builder (Contractor) | PROD & DEV | +1-555-0103 | **+15125553003** | 512 (Austin, TX) |

### Format Details

- **Old Format**: `+1-555-0101` (11 characters with dashes)
- **New Format**: `+12175551001` (12 characters, no dashes)
- **Pattern**: `+1` + `Area Code` + `555` + `4-digit unique number`

### Area Codes Used

Each phone number now uses a realistic area code matching the user's location:
- **217**: Springfield, Illinois (John Smith)
- **503**: Portland, Oregon (Jane Doe)
- **512**: Austin, Texas (Bob Builder)

### Updated Fields

For each of the 6 demo users (3 PROD + 3 DEV), the following fields were updated:
1. `PhoneNumber` property on the User object
2. `Phone` property in the `ContactInformation` object

### Verification

✅ Build successful - all changes compile without errors
✅ All 6 users updated (3 PROD + 3 DEV environments)
✅ Phone numbers are unique and realistic
✅ Format matches requirement: +19999999999 (12 characters)

## Testing

To test the updated phone numbers:

```bash
# Seed the database
dotnet run --project src/HippoExchange.Api/ seed

# Check a user's phone number
curl http://localhost:8080/users/user_33UeIDzYloCoZABaaCR1WPmV7MT
```

Expected response should include:
```json
{
  "phone_number": "+12175551001",
  "contact_information": {
    "phone": "+12175551001"
  }
}
```

## Files Modified

- `/workspace/src/HippoExchange.Api/Services/DatabaseSeeder.cs`
  - Updated 6 user definitions (3 PROD + 3 DEV)
  - Changed 12 phone number strings total (2 per user)
