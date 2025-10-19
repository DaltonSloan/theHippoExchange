# Database Seeding Architecture

> **Authentication update:** Legacy references to the `X-User-Id` header in this diagram have been superseded by standard `Authorization: Bearer <token>` authentication handled by Clerk.

```
┌─────────────────────────────────────────────────────────────────┐
│                      HippoExchange Application                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                         Entry Points                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Command Line                2. REST API Endpoints            │
│     ┌──────────────┐              ┌─────────────────────┐      │
│     │ dotnet run   │              │ POST /api/admin/    │      │
│     │    seed      │              │      seed           │      │
│     └──────┬───────┘              └──────────┬──────────┘      │
│            │                                  │                 │
│     ┌──────┴───────┐              ┌──────────┴──────────┐      │
│     │ dotnet run   │              │ POST /api/admin/    │      │
│     │   reset      │              │      reset          │      │
│     └──────────────┘              └─────────────────────┘      │
│                                   ┌─────────────────────┐      │
│                                   │ DELETE /api/admin/  │      │
│                                   │        seed         │      │
│                                   └─────────────────────┘      │
│                                   ┌─────────────────────┐      │
│                                   │ GET /api/admin/seed │      │
│                                   │      /status        │      │
│                                   └─────────────────────┘      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       DatabaseSeeder Service                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  SeedDatabaseAsync()                                    │    │
│  │  - Clears existing demo data                            │    │
│  │  - Creates 3 demo users                                 │    │
│  │  - Creates 5-10 assets per user                         │    │
│  │  - Creates 8-15 maintenance records per asset           │    │
│  │  - Updates user statistics                              │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  ResetDatabaseAsync()                                   │    │
│  │  - Drops all collections                                │    │
│  │  - Calls SeedDatabaseAsync()                            │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐    │
│  │  ClearDemoDataAsync()                                   │    │
│  │  - Identifies demo users by Clerk ID                    │    │
│  │  - Deletes demo assets                                  │    │
│  │  - Deletes demo maintenance records                     │    │
│  │  - Deletes demo users                                   │    │
│  └────────────────────────────────────────────────────────┘    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      MongoDB Collections                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐     │
│  │    users     │    │    assets    │    │ maintenance  │     │
│  ├──────────────┤    ├──────────────┤    ├──────────────┤     │
│  │ John Smith   │◄───│ Lawn Mower   │◄───│ Oil Change   │     │
│  │ Jane Doe     │    │ Table Saw    │    │ Blade Sharp  │     │
│  │ Bob Builder  │    │ Nailer       │    │ Filter Check │     │
│  └──────────────┘    └──────────────┘    └──────────────┘     │
│                                                                  │
│  Total Records After Seeding:                                   │
│  - 3 users                                                      │
│  - 26 assets (7 + 9 + 10)                                       │
│  - ~315 maintenance records                                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│                       Demo Users Created                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  User 1: John Smith                                             │
│  ├─ Clerk ID: clerk_john_smith                                  │
│  ├─ Persona: Homeowner                                          │
│  ├─ Location: Springfield, IL                                   │
│  ├─ Assets: 7 (lawn & garden equipment)                         │
│  │   ├─ Push Lawn Mower (Honda) - $449.99                       │
│  │   ├─ Gas-Powered Leaf Blower (Echo) - $199.99               │
│  │   ├─ Electric Hedge Trimmer (Black+Decker) - $79.99         │
│  │   ├─ Pressure Washer (Ryobi) - $299.99                      │
│  │   ├─ Garden Hose Reel (Craftsman) - $59.99                  │
│  │   ├─ Electric Chainsaw (Husqvarna) - $189.99                │
│  │   └─ Extension Ladder 24ft (Werner) - $249.99               │
│  └─ Maintenance Records: ~85                                    │
│                                                                  │
│  User 2: Jane Doe                                               │
│  ├─ Clerk ID: clerk_jane_doe                                    │
│  ├─ Persona: Hobbyist                                           │
│  ├─ Location: Portland, OR                                      │
│  ├─ Assets: 9 (workshop tools)                                  │
│  │   ├─ Table Saw (DeWalt) - $599.99                            │
│  │   ├─ Cordless Drill Driver Set (Makita) - $179.99           │
│  │   ├─ Miter Saw (Bosch) - $399.99                            │
│  │   ├─ Palm Sander (Black+Decker) - $49.99                    │
│  │   ├─ Shop Vacuum (Shop-Vac) - $89.99                        │
│  │   ├─ Router with Table (Porter-Cable) - $259.99             │
│  │   ├─ Air Compressor (Craftsman) - $199.99                   │
│  │   ├─ Jigsaw (Makita) - $129.99                              │
│  │   └─ Workbench with Vise (Husky) - $349.99                  │
│  └─ Maintenance Records: ~102                                   │
│                                                                  │
│  User 3: Bob Builder                                            │
│  ├─ Clerk ID: clerk_bob_builder                                 │
│  ├─ Persona: Professional Contractor                            │
│  ├─ Location: Austin, TX                                        │
│  ├─ Assets: 10 (professional equipment)                         │
│  │   ├─ Professional Framing Nailer (Milwaukee) - $449.99      │
│  │   ├─ Impact Driver Kit (DeWalt) - $299.99                   │
│  │   ├─ Tile Saw (Ridgid) - $799.99                            │
│  │   ├─ Rotary Hammer Drill (Bosch) - $349.99                  │
│  │   ├─ Laser Level (DeWalt) - $499.99                         │
│  │   ├─ Circular Saw (Makita) - $179.99                        │
│  │   ├─ Scaffold Set (Werner) - $1,499.99                      │
│  │   ├─ Concrete Mixer (Honda) - $899.99                       │
│  │   ├─ Welding Machine (Lincoln Electric) - $1,299.99         │
│  │   └─ Generator 7500W (Champion) - $1,099.99                 │
│  └─ Maintenance Records: ~128                                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│                    Maintenance Distribution                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Each asset gets 8-15 maintenance records with:                 │
│                                                                  │
│  ████████ 20% - Overdue (past due date)                         │
│  ██████ 15% - Due Soon (within 7 days)                          │
│  ████████ 20% - Due Later (within 30 days)                      │
│  ██████████████████ 45% - Completed                             │
│                                                                  │
│  Task Types by Category:                                        │
│  ├─ Lawn Equipment: Oil changes, blade sharpening, filters     │
│  ├─ Power Tools: Blade replacement, calibration, battery       │
│  ├─ Cleaning: Filter replacement, hose inspection              │
│  └─ General: Safety checks, cleaning, storage                  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│                         Data Flow                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. User triggers seeding (CLI or API)                          │
│           ↓                                                     │
│  2. DatabaseSeeder.SeedDatabaseAsync() called                   │
│           ↓                                                     │
│  3. ClearDemoDataAsync() removes existing demo users            │
│           ↓                                                     │
│  4. CreateDemoUsersAsync() creates 3 users                      │
│           ↓                                                     │
│  5. For each user:                                              │
│      ├─ CreateDemoAssetsForUserAsync() creates 5-10 assets     │
│      │        ↓                                                 │
│      └─ For each asset:                                         │
│           └─ CreateDemoMaintenanceForAssetAsync()              │
│              creates 8-15 maintenance records                   │
│           ↓                                                     │
│  6. Update user statistics                                      │
│           ↓                                                     │
│  7. Return success response                                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────┐
│                    Integration Points                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Swagger UI                                                     │
│  http://localhost:8080/swagger                                  │
│  └─ Admin Tag contains all seeding endpoints                   │
│                                                                  │
│  Mongo Express                                                  │
│  http://localhost:8081                                          │
│  └─ View seeded data directly in database                      │
│                                                                  │
│  API Testing                                                    │
│  └─ Use Clerk IDs in X-User-Id header:                         │
│      ├─ clerk_john_smith                                        │
│      ├─ clerk_jane_doe                                          │
│      └─ clerk_bob_builder                                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

### 1. Idempotent by Design
- Demo users identified by specific Clerk IDs
- Existing demo data removed before seeding
- Safe to run multiple times

### 2. Realistic Data
- Purchase dates spread over several years
- Maintenance dates relative to current date
- Varied brands and price ranges
- Category-specific maintenance tasks

### 3. Dual Interface
- Command-line for developers
- REST API for automation/testing
- Both use same underlying service

### 4. Flexible Cleanup
- Full reset (deletes everything)
- Seed (removes only demo data)
- Purge (removes demo data without re-seeding)

### 5. Observable
- Comprehensive logging
- Status check endpoint
- Detailed error messages
