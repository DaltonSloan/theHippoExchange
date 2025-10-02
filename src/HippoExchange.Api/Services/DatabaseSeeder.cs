using HippoExchange.Models;
using HippoExchange.Models.Clerk;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using HippoExchange.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HippoExchange.Services
{
    /// <summary>
    /// Service for seeding the database with demo data for development and testing.
    /// Creates realistic demo users, assets, and maintenance records.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Assets> _assetsCollection;
        private readonly IMongoCollection<Maintenance> _maintenanceCollection;
        private readonly IMongoDatabase _database;

        public DatabaseSeeder(IOptions<MongoSettings> mongoSettings)
        {
            var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
            _database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            _usersCollection = _database.GetCollection<User>("users");
            _assetsCollection = _database.GetCollection<Assets>("assets");
            _maintenanceCollection = _database.GetCollection<Maintenance>("maintenance");
        }

        /// <summary>
        /// Seeds the database with demo data. This operation is idempotent - running it
        /// multiple times will not create duplicates. Existing demo users will be removed
        /// and recreated.
        /// </summary>
        public async Task SeedDatabaseAsync()
        {
            Console.WriteLine("🌱 Starting database seeding...");
            
            // Clear existing demo data
            await ClearDemoDataAsync();
            
            // Create demo users
            var users = await CreateDemoUsersAsync();
            Console.WriteLine($"✅ Created {users.Count} demo users");
            
            // Create assets for each user
            var assetCount = 0;
            foreach (var user in users)
            {
                var assets = await CreateDemoAssetsForUserAsync(user);
                assetCount += assets.Count;
                Console.WriteLine($"✅ Created {assets.Count} assets for {user.FirstName} {user.LastName}");
                
                // Create maintenance records for each asset
                var maintenanceCount = 0;
                foreach (var asset in assets)
                {
                    var maintenanceRecords = await CreateDemoMaintenanceForAssetAsync(asset);
                    maintenanceCount += maintenanceRecords.Count;
                }
                Console.WriteLine($"✅ Created {maintenanceCount} maintenance records for {user.FirstName}'s assets");
            }
            
            Console.WriteLine($"\n🎉 Database seeding complete!");
            Console.WriteLine($"   - {users.Count} demo users");
            Console.WriteLine($"   - {assetCount} total assets");
            Console.WriteLine($"\nDemo User Credentials:");
            Console.WriteLine("   User 1: user_33UeIDzYloCoZABaaCR1WPmV7MT (John Smith - Homeowner)");
            Console.WriteLine("   User 2: user_33UeKv6eNbmLb2HClHd1PN51AZ5 (Jane Doe - Hobbyist)");
            Console.WriteLine("   User 3: user_33UeOCZ7LGxjHJ8dkwnAIozslO0 (Bob Builder - Contractor)");
            Console.WriteLine("\nUse these Clerk IDs in the X-User-Id header for API testing.");
        }

        /// <summary>
        /// Clears all demo data from the database. Identifies demo users by their
        /// Clerk IDs starting with "clerk_demo_" or specific known demo IDs.
        /// </summary>
        public async Task ClearDemoDataAsync()
        {
            Console.WriteLine("🧹 Clearing existing demo data...");
            
            var demoClerkIds = new[] { 
                "user_33UeIDzYloCoZABaaCR1WPmV7MT",  // john_smith
                "user_33UeKv6eNbmLb2HClHd1PN51AZ5",  // jane_doe
                "user_33UeOCZ7LGxjHJ8dkwnAIozslO0"   // bob_builder
            };
            
            // Find demo users
            var demoUsers = await _usersCollection
                .Find(u => demoClerkIds.Contains(u.ClerkId))
                .ToListAsync();
            
            if (demoUsers.Any())
            {
                var demoUserClerkIds = demoUsers.Select(u => u.ClerkId).ToList();
                
                // Delete assets owned by demo users
                await _assetsCollection.DeleteManyAsync(a => demoUserClerkIds.Contains(a.OwnerUserId));
                
                // Delete maintenance records for demo assets
                var demoAssetIds = await _assetsCollection
                    .Find(a => demoUserClerkIds.Contains(a.OwnerUserId))
                    .Project(a => a.Id)
                    .ToListAsync();
                
                if (demoAssetIds.Any())
                {
                    await _maintenanceCollection.DeleteManyAsync(m => demoAssetIds.Contains(m.AssetId));
                }
                
                // Delete demo users
                await _usersCollection.DeleteManyAsync(u => demoClerkIds.Contains(u.ClerkId));
                
                Console.WriteLine($"   Removed {demoUsers.Count} demo users and their associated data");
            }
        }

        /// <summary>
        /// Creates three demo users with different personas:
        /// 1. John Smith - Homeowner with lawn/garden equipment
        /// 2. Jane Doe - Hobbyist with workshop tools
        /// 3. Bob Builder - Contractor with professional equipment
        /// </summary>
        private async Task<List<User>> CreateDemoUsersAsync()
        {
            var users = new List<User>
            {
                // User 1: John Smith - Homeowner
                new User
                {
                    ClerkId = "user_33UeIDzYloCoZABaaCR1WPmV7MT",
                    Email = "john.smith@demo.hippoexchange.com",
                    Username = "john_smith",
                    FirstName = "John",
                    LastName = "Smith",
                    FullName = "John Smith",
                    ProfileImageUrl = "https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVJRHpZbG9Db1pBQmFhQ1IxV1BtVjdNVCIsImluaXRpYWxzIjoiSlMifQ",
                    Location = "Springfield, IL",
                    PhoneNumber = "+1-555-0101",
                    ImageUrl = "https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVJRHpZbG9Db1pBQmFhQ1IxV1BtVjdNVCIsImluaXRpYWxzIjoiSlMifQ",
                    HasImage = false,
                    PrimaryEmailAddressId = "idn_33UeI8ZFWT796TFQuscbvSCXayJ",
                    LastSignInAt = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds(),
                    UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Address = new Address
                    {
                        Street = "123 Maple Street",
                        City = "Springfield",
                        State = "IL",
                        PostalCode = "62701",
                        Country = "USA"
                    },
                    ContactInformation = new ContactInformation
                    {
                        Email = "john.smith@demo.hippoexchange.com",
                        Phone = "+1-555-0101",
                        PreferredContactMethod = "email"
                    },
                    AccountStatus = new AccountStatus
                    {
                        EmailVerified = true,
                        AccountActive = true,
                        Banned = false,
                        Locked = false
                    },
                    Statistics = new Statistics
                    {
                        TotalAssets = 0, // Will be updated later
                        AssetsLoaned = 0,
                        AssetsBorrowed = 0
                    },
                    EmailAddresses = new List<ClerkEmailAddress>
                    {
                        new ClerkEmailAddress
                        {
                            Id = "idn_33UeI8ZFWT796TFQuscbvSCXayJ",
                            EmailAddress = "john.smith@demo.hippoexchange.com",
                            Verification = new ClerkVerification { Status = "verified", Strategy = "admin" }
                        }
                    }
                },
                
                // User 2: Jane Doe - Hobbyist
                new User
                {
                    ClerkId = "user_33UeKv6eNbmLb2HClHd1PN51AZ5",
                    Email = "jane.doe@demo.hippoexchange.com",
                    Username = "jane_doe",
                    FirstName = "Jane",
                    LastName = "Doe",
                    FullName = "Jane Doe",
                    ProfileImageUrl = "https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVLdjZlTmJtTGIySENsSGQxUE41MUFaNSIsImluaXRpYWxzIjoiSkQifQ",
                    Location = "Portland, OR",
                    PhoneNumber = "+1-555-0102",
                    ImageUrl = "https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVLdjZlTmJtTGIySENsSGQxUE41MUFaNSIsImluaXRpYWxzIjoiSkQifQ",
                    HasImage = false,
                    PrimaryEmailAddressId = "idn_33UeKuwQnPVVaByJV4qZu4DXnuQ",
                    LastSignInAt = DateTimeOffset.UtcNow.AddHours(-6).ToUnixTimeMilliseconds(),
                    UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Address = new Address
                    {
                        Street = "456 Oak Avenue",
                        City = "Portland",
                        State = "OR",
                        PostalCode = "97201",
                        Country = "USA"
                    },
                    ContactInformation = new ContactInformation
                    {
                        Email = "jane.doe@demo.hippoexchange.com",
                        Phone = "+1-555-0102",
                        PreferredContactMethod = "phone"
                    },
                    AccountStatus = new AccountStatus
                    {
                        EmailVerified = true,
                        AccountActive = true,
                        Banned = false,
                        Locked = false
                    },
                    Statistics = new Statistics
                    {
                        TotalAssets = 0,
                        AssetsLoaned = 0,
                        AssetsBorrowed = 0
                    },
                    EmailAddresses = new List<ClerkEmailAddress>
                    {
                        new ClerkEmailAddress
                        {
                            Id = "idn_33UeKuwQnPVVaByJV4qZu4DXnuQ",
                            EmailAddress = "jane.doe@demo.hippoexchange.com",
                            Verification = new ClerkVerification { Status = "verified", Strategy = "admin" }
                        }
                    }
                },
                
                // User 3: Bob Builder - Contractor
                new User
                {
                    ClerkId = "user_33UeOCZ7LGxjHJ8dkwnAIozslO0",
                    Email = "bob.builder@demo.hippoexchange.com",
                    Username = "bob_builder",
                    FirstName = "Bob",
                    LastName = "Builder",
                    FullName = "Bob Builder",
                    ProfileImageUrl = "https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVPQ1o3TEd4akhKOGRrd25BSW96c2xPMCIsImluaXRpYWxzIjoiQkIifQ",
                    Location = "Austin, TX",
                    PhoneNumber = "+1-555-0103",
                    ImageUrl = "https://img.clerk.com/eyJ0eXBlIjoiZGVmYXVsdCIsImlpZCI6Imluc18zMkNBNVUxTHJxc1Y2amVqcFBGVmIwZTBVTlYiLCJyaWQiOiJ1c2VyXzMzVWVPQ1o3TEd4akhKOGRrd25BSW96c2xPMCIsImluaXRpYWxzIjoiQkIifQ",
                    HasImage = false,
                    PrimaryEmailAddressId = "idn_33UeOAgYquzo8uNfv9risd0VOmO",
                    LastSignInAt = DateTimeOffset.UtcNow.AddMinutes(-30).ToUnixTimeMilliseconds(),
                    UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Address = new Address
                    {
                        Street = "789 Construction Blvd",
                        City = "Austin",
                        State = "TX",
                        PostalCode = "78701",
                        Country = "USA"
                    },
                    ContactInformation = new ContactInformation
                    {
                        Email = "bob.builder@demo.hippoexchange.com",
                        Phone = "+1-555-0103",
                        PreferredContactMethod = "phone"
                    },
                    AccountStatus = new AccountStatus
                    {
                        EmailVerified = true,
                        AccountActive = true,
                        Banned = false,
                        Locked = false
                    },
                    Statistics = new Statistics
                    {
                        TotalAssets = 0,
                        AssetsLoaned = 0,
                        AssetsBorrowed = 0
                    },
                    EmailAddresses = new List<ClerkEmailAddress>
                    {
                        new ClerkEmailAddress
                        {
                            Id = "idn_33UeOAgYquzo8uNfv9risd0VOmO",
                            EmailAddress = "bob.builder@demo.hippoexchange.com",
                            Verification = new ClerkVerification { Status = "verified", Strategy = "admin" }
                        }
                    }
                }
            };

            await _usersCollection.InsertManyAsync(users);
            return users;
        }

        /// <summary>
        /// Creates demo assets for a specific user based on their persona.
        /// Each user gets 5-10 assets appropriate to their profile.
        /// </summary>
        private async Task<List<Assets>> CreateDemoAssetsForUserAsync(User user)
        {
            var assets = new List<Assets>();
            
            switch (user.ClerkId)
            {
                case "user_33UeIDzYloCoZABaaCR1WPmV7MT":  // john_smith
                    // Homeowner with lawn/garden equipment
                    assets = new List<Assets>
                    {
                        new Assets
                        {
                            ItemName = "Push Lawn Mower",
                            BrandName = "Honda",
                            Category = "Lawn & Garden",
                            PurchaseDate = DateTime.UtcNow.AddYears(-2).AddMonths(-3),
                            PurchaseCost = 449.99m,
                            CurrentLocation = "Garage - Left Side",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Honda+Lawn+Mower" },
                            ConditionDescription = "Good condition, well-maintained. Some minor cosmetic wear.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Gas-Powered Leaf Blower",
                            BrandName = "Echo",
                            Category = "Lawn & Garden",
                            PurchaseDate = DateTime.UtcNow.AddYears(-1).AddMonths(-6),
                            PurchaseCost = 199.99m,
                            CurrentLocation = "Garage - Tool Wall",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Echo+Leaf+Blower" },
                            ConditionDescription = "Excellent condition, barely used.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Electric Hedge Trimmer",
                            BrandName = "Black+Decker",
                            Category = "Lawn & Garden",
                            PurchaseDate = DateTime.UtcNow.AddMonths(-8),
                            PurchaseCost = 79.99m,
                            CurrentLocation = "Shed",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Hedge+Trimmer" },
                            ConditionDescription = "Good working condition.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Pressure Washer",
                            BrandName = "Ryobi",
                            Category = "Cleaning",
                            PurchaseDate = DateTime.UtcNow.AddYears(-1),
                            PurchaseCost = 299.99m,
                            CurrentLocation = "Garage - Back Corner",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Pressure+Washer" },
                            ConditionDescription = "Works great, cleaned deck and driveway last month.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Garden Hose Reel",
                            BrandName = "Craftsman",
                            Category = "Lawn & Garden",
                            PurchaseDate = DateTime.UtcNow.AddMonths(-4),
                            PurchaseCost = 59.99m,
                            CurrentLocation = "Side of House",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Hose+Reel" },
                            ConditionDescription = "Like new.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Electric Chainsaw",
                            BrandName = "Husqvarna",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-3),
                            PurchaseCost = 189.99m,
                            CurrentLocation = "Garage - High Shelf",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Chainsaw" },
                            ConditionDescription = "Chain needs sharpening but otherwise functional.",
                            OwnerUserId = user.ClerkId,
                            Status = "maintenance",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Extension Ladder 24ft",
                            BrandName = "Werner",
                            Category = "Ladders",
                            PurchaseDate = DateTime.UtcNow.AddYears(-5),
                            PurchaseCost = 249.99m,
                            CurrentLocation = "Garage - Wall Mount",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Extension+Ladder" },
                            ConditionDescription = "Solid and safe, some paint wear.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        }
                    };
                    break;
                    
                case "user_33UeKv6eNbmLb2HClHd1PN51AZ5":  // jane_doe
                    // Hobbyist with workshop tools
                    assets = new List<Assets>
                    {
                        new Assets
                        {
                            ItemName = "Table Saw",
                            BrandName = "DeWalt",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-1).AddMonths(-9),
                            PurchaseCost = 599.99m,
                            CurrentLocation = "Workshop - Center",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=DeWalt+Table+Saw" },
                            ConditionDescription = "Excellent condition, blade recently replaced.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Cordless Drill Driver Set",
                            BrandName = "Makita",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-2),
                            PurchaseCost = 179.99m,
                            CurrentLocation = "Workshop - Tool Cabinet",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Makita+Drill" },
                            ConditionDescription = "Great condition, comes with two batteries.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Miter Saw",
                            BrandName = "Bosch",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-1),
                            PurchaseCost = 399.99m,
                            CurrentLocation = "Workshop - Right Wall",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Bosch+Miter+Saw" },
                            ConditionDescription = "Perfect condition, laser guide works perfectly.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Palm Sander",
                            BrandName = "Black+Decker",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddMonths(-6),
                            PurchaseCost = 49.99m,
                            CurrentLocation = "Workshop - Tool Box",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Palm+Sander" },
                            ConditionDescription = "Works well, gets the job done.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Shop Vacuum",
                            BrandName = "Shop-Vac",
                            Category = "Cleaning",
                            PurchaseDate = DateTime.UtcNow.AddYears(-3),
                            PurchaseCost = 89.99m,
                            CurrentLocation = "Workshop - Corner",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Shop+Vac" },
                            ConditionDescription = "Still going strong after years of use.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Router with Table",
                            BrandName = "Porter-Cable",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-2).AddMonths(-6),
                            PurchaseCost = 259.99m,
                            CurrentLocation = "Workshop - Back Bench",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Router+Table" },
                            ConditionDescription = "Good condition, multiple router bits included.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Air Compressor",
                            BrandName = "Craftsman",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-4),
                            PurchaseCost = 199.99m,
                            CurrentLocation = "Workshop - Near Door",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Air+Compressor" },
                            ConditionDescription = "Reliable, recently serviced.",
                            OwnerUserId = user.ClerkId,
                            Status = "maintenance",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Jigsaw",
                            BrandName = "Makita",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddMonths(-10),
                            PurchaseCost = 129.99m,
                            CurrentLocation = "Workshop - Tool Cabinet",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Jigsaw" },
                            ConditionDescription = "Excellent for curved cuts, very accurate.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Workbench with Vise",
                            BrandName = "Husky",
                            Category = "Workshop Furniture",
                            PurchaseDate = DateTime.UtcNow.AddYears(-5),
                            PurchaseCost = 349.99m,
                            CurrentLocation = "Workshop - Main Work Area",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Workbench" },
                            ConditionDescription = "Sturdy and reliable, well-worn but solid.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        }
                    };
                    break;
                    
                case "user_33UeOCZ7LGxjHJ8dkwnAIozslO0":  // bob_builder
                    // Contractor with professional equipment
                    assets = new List<Assets>
                    {
                        new Assets
                        {
                            ItemName = "Professional Framing Nailer",
                            BrandName = "Milwaukee",
                            Category = "Pneumatic Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-1).AddMonths(-3),
                            PurchaseCost = 449.99m,
                            CurrentLocation = "Work Truck - Tool Box",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Milwaukee+Nailer" },
                            ConditionDescription = "Professional grade, used daily on job sites.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Impact Driver Kit",
                            BrandName = "DeWalt",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-2),
                            PurchaseCost = 299.99m,
                            CurrentLocation = "Work Truck - Front Seat",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Impact+Driver" },
                            ConditionDescription = "Workhorse tool, reliable and powerful.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Tile Saw",
                            BrandName = "Ridgid",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-3),
                            PurchaseCost = 799.99m,
                            CurrentLocation = "Storage Unit",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Tile+Saw" },
                            ConditionDescription = "Heavy duty, perfect for large tile jobs.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Rotary Hammer Drill",
                            BrandName = "Bosch",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-1).AddMonths(-8),
                            PurchaseCost = 349.99m,
                            CurrentLocation = "Work Truck - Tool Box",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Hammer+Drill" },
                            ConditionDescription = "Professional quality, essential for concrete work.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Laser Level",
                            BrandName = "DeWalt",
                            Category = "Measuring Tools",
                            PurchaseDate = DateTime.UtcNow.AddMonths(-9),
                            PurchaseCost = 499.99m,
                            CurrentLocation = "Work Truck - Tool Box",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Laser+Level" },
                            ConditionDescription = "Accurate and reliable, red beam laser.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Circular Saw",
                            BrandName = "Makita",
                            Category = "Power Tools",
                            PurchaseDate = DateTime.UtcNow.AddYears(-4),
                            PurchaseCost = 179.99m,
                            CurrentLocation = "Work Truck - Tool Box",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Circular+Saw" },
                            ConditionDescription = "Reliable saw, blade needs replacement soon.",
                            OwnerUserId = user.ClerkId,
                            Status = "maintenance",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Scaffold Set (4 Sections)",
                            BrandName = "Werner",
                            Category = "Scaffolding",
                            PurchaseDate = DateTime.UtcNow.AddYears(-6),
                            PurchaseCost = 1499.99m,
                            CurrentLocation = "Storage Unit",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Scaffold" },
                            ConditionDescription = "Commercial grade, inspected annually.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Concrete Mixer",
                            BrandName = "Honda",
                            Category = "Heavy Equipment",
                            PurchaseDate = DateTime.UtcNow.AddYears(-5),
                            PurchaseCost = 899.99m,
                            CurrentLocation = "Storage Yard",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Concrete+Mixer" },
                            ConditionDescription = "Gas powered, starts reliably.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = false
                        },
                        new Assets
                        {
                            ItemName = "Welding Machine",
                            BrandName = "Lincoln Electric",
                            Category = "Welding Equipment",
                            PurchaseDate = DateTime.UtcNow.AddYears(-7),
                            PurchaseCost = 1299.99m,
                            CurrentLocation = "Workshop",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Welder" },
                            ConditionDescription = "Professional MIG welder, excellent condition.",
                            OwnerUserId = user.ClerkId,
                            Status = "available",
                            Favorite = true
                        },
                        new Assets
                        {
                            ItemName = "Generator 7500W",
                            BrandName = "Champion",
                            Category = "Power Equipment",
                            PurchaseDate = DateTime.UtcNow.AddYears(-2).AddMonths(-6),
                            PurchaseCost = 1099.99m,
                            CurrentLocation = "Work Truck - Bed",
                            Images = new List<string> { "https://via.placeholder.com/400x300?text=Generator" },
                            ConditionDescription = "Reliable power source for remote job sites.",
                            OwnerUserId = user.ClerkId,
                            Status = "maintenance",
                            Favorite = false
                        }
                    };
                    break;
            }

            await _assetsCollection.InsertManyAsync(assets);
            
            // Update user statistics
            var filter = Builders<User>.Filter.Eq(u => u.ClerkId, user.ClerkId);
            var update = Builders<User>.Update.Set(u => u.Statistics.TotalAssets, assets.Count);
            await _usersCollection.UpdateOneAsync(filter, update);
            
            return assets;
        }

        /// <summary>
        /// Creates 8-15 maintenance records for a given asset with varied statuses and due dates.
        /// Includes overdue, due soon, due later, and completed maintenance tasks.
        /// </summary>
        private async Task<List<Maintenance>> CreateDemoMaintenanceForAssetAsync(Assets asset)
        {
            var random = new Random(asset.ItemName.GetHashCode()); // Deterministic random based on asset name
            var maintenanceCount = random.Next(8, 16); // 8-15 maintenance records
            var maintenanceRecords = new List<Maintenance>();
            
            // Define possible maintenance tasks based on asset category
            var maintenanceTasks = GetMaintenanceTasksForCategory(asset.Category, asset.ItemName);
            
            for (int i = 0; i < maintenanceCount; i++)
            {
                var taskIndex = i % maintenanceTasks.Count;
                var task = maintenanceTasks[taskIndex];
                
                // Vary the due dates and statuses
                var (dueDate, status) = GenerateMaintenanceSchedule(i, maintenanceCount);
                
                var maintenance = new Maintenance
                {
                    AssetId = asset.Id!,
                    BrandName = asset.BrandName,
                    ProductName = asset.ItemName,
                    PurchaseLocation = GetRandomPurchaseLocation(random),
                    CostPaid = asset.PurchaseCost,
                    MaintenanceDueDate = dueDate,
                    MaintenanceTitle = task.Title,
                    MaintenanceDescription = task.Description,
                    MaintenanceStatus = status,
                    PreserveFromPrior = status == "completed",
                    RequiredTools = task.RequiredTools,
                    ToolLocation = asset.CurrentLocation
                };
                
                maintenanceRecords.Add(maintenance);
            }
            
            await _maintenanceCollection.InsertManyAsync(maintenanceRecords);
            return maintenanceRecords;
        }

        /// <summary>
        /// Generates realistic maintenance schedules with varied due dates and statuses.
        /// Creates a mix of overdue, due soon, due later, and completed tasks.
        /// </summary>
        private (DateTime dueDate, string status) GenerateMaintenanceSchedule(int index, int totalCount)
        {
            var now = DateTime.UtcNow;
            
            // Distribute maintenance tasks across different time periods
            var segment = (double)index / totalCount;
            
            if (segment < 0.2) // 20% overdue
            {
                var daysOverdue = (index + 1) * 5; // 5, 10, 15, etc. days overdue
                return (now.AddDays(-daysOverdue), "overdue");
            }
            else if (segment < 0.35) // 15% due soon (within 7 days)
            {
                var daysUntilDue = (index % 7) + 1;
                return (now.AddDays(daysUntilDue), "pending");
            }
            else if (segment < 0.55) // 20% due later (within 30 days)
            {
                var daysUntilDue = (index % 23) + 8; // 8-30 days
                return (now.AddDays(daysUntilDue), "pending");
            }
            else // 45% completed
            {
                var daysAgo = (index % 180) + 1; // Completed in last 6 months
                return (now.AddDays(-daysAgo), "completed");
            }
        }

        /// <summary>
        /// Returns appropriate maintenance tasks based on asset category.
        /// </summary>
        private List<MaintenanceTask> GetMaintenanceTasksForCategory(string category, string itemName)
        {
            var tasks = new List<MaintenanceTask>();
            
            switch (category.ToLower())
            {
                case "lawn & garden":
                    tasks = new List<MaintenanceTask>
                    {
                        new MaintenanceTask
                        {
                            Title = "Oil Change",
                            Description = "Change engine oil and replace oil filter. Use SAE 30 or 10W-30 oil.",
                            RequiredTools = "Oil drain pan, wrench set, new oil filter"
                        },
                        new MaintenanceTask
                        {
                            Title = "Sharpen Blades",
                            Description = "Remove and sharpen cutting blades for optimal performance.",
                            RequiredTools = "Socket wrench, bench grinder or file, safety gloves"
                        },
                        new MaintenanceTask
                        {
                            Title = "Air Filter Replacement",
                            Description = "Replace air filter to ensure proper engine performance.",
                            RequiredTools = "Screwdriver, replacement air filter"
                        },
                        new MaintenanceTask
                        {
                            Title = "Spark Plug Replacement",
                            Description = "Replace spark plug to maintain easy starting and smooth operation.",
                            RequiredTools = "Spark plug wrench, gap gauge, new spark plug"
                        },
                        new MaintenanceTask
                        {
                            Title = "Clean Deck/Housing",
                            Description = "Remove grass buildup from underside of deck to prevent rust and maintain performance.",
                            RequiredTools = "Putty knife, wire brush, garden hose"
                        }
                    };
                    break;
                    
                case "power tools":
                    tasks = new List<MaintenanceTask>
                    {
                        new MaintenanceTask
                        {
                            Title = "Blade Replacement",
                            Description = "Replace worn or dull blade with appropriate replacement for material being cut.",
                            RequiredTools = "Allen wrenches, new blade, safety glasses"
                        },
                        new MaintenanceTask
                        {
                            Title = "Clean and Lubricate",
                            Description = "Clean tool exterior and moving parts, apply appropriate lubricant.",
                            RequiredTools = "Compressed air, cleaning cloth, tool lubricant"
                        },
                        new MaintenanceTask
                        {
                            Title = "Check Power Cord",
                            Description = "Inspect power cord for damage, replace if frayed or damaged.",
                            RequiredTools = "Visual inspection, replacement cord if needed"
                        },
                        new MaintenanceTask
                        {
                            Title = "Battery Maintenance",
                            Description = "Check battery charge, clean contacts, ensure proper storage.",
                            RequiredTools = "Battery charger, contact cleaner, soft cloth"
                        },
                        new MaintenanceTask
                        {
                            Title = "Calibration Check",
                            Description = "Verify tool accuracy and adjust as needed for precise operation.",
                            RequiredTools = "Square, measuring tape, adjustment tools"
                        },
                        new MaintenanceTask
                        {
                            Title = "Dust Collection System",
                            Description = "Clean or replace dust collection bag/filter for optimal performance.",
                            RequiredTools = "Replacement bag/filter, vacuum"
                        }
                    };
                    break;
                    
                case "cleaning":
                    tasks = new List<MaintenanceTask>
                    {
                        new MaintenanceTask
                        {
                            Title = "Replace Filters",
                            Description = "Replace HEPA or foam filters to maintain suction power.",
                            RequiredTools = "Replacement filters"
                        },
                        new MaintenanceTask
                        {
                            Title = "Check Hoses",
                            Description = "Inspect hoses for cracks or clogs, clean or replace as needed.",
                            RequiredTools = "Flashlight, replacement hose if damaged"
                        },
                        new MaintenanceTask
                        {
                            Title = "Empty and Clean Tank",
                            Description = "Empty collection tank and clean interior to prevent odors.",
                            RequiredTools = "Water, mild detergent, cleaning cloth"
                        },
                        new MaintenanceTask
                        {
                            Title = "Nozzle Inspection",
                            Description = "Check nozzles for wear or damage, replace if necessary.",
                            RequiredTools = "Replacement nozzles, wrench"
                        }
                    };
                    break;
                    
                default:
                    tasks = new List<MaintenanceTask>
                    {
                        new MaintenanceTask
                        {
                            Title = "General Inspection",
                            Description = "Perform visual inspection for wear, damage, or needed repairs.",
                            RequiredTools = "Visual inspection, notepad"
                        },
                        new MaintenanceTask
                        {
                            Title = "Cleaning and Storage",
                            Description = "Clean equipment and ensure proper storage to prevent damage.",
                            RequiredTools = "Cleaning supplies, storage area"
                        },
                        new MaintenanceTask
                        {
                            Title = "Safety Check",
                            Description = "Verify all safety features are functioning properly.",
                            RequiredTools = "Visual inspection, test equipment"
                        }
                    };
                    break;
            }
            
            return tasks;
        }

        /// <summary>
        /// Returns a random purchase location for maintenance records.
        /// </summary>
        private string GetRandomPurchaseLocation(Random random)
        {
            var locations = new[]
            {
                "Home Depot",
                "Lowe's",
                "Ace Hardware",
                "Harbor Freight",
                "Menards",
                "Tractor Supply Co.",
                "Northern Tool",
                "Amazon",
                "Local Hardware Store"
            };
            
            return locations[random.Next(locations.Length)];
        }

        /// <summary>
        /// Helper class to represent maintenance tasks with their details.
        /// </summary>
        private class MaintenanceTask
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string RequiredTools { get; set; } = string.Empty;
        }
    }
}
