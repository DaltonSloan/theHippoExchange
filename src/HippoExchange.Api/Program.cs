using HippoExchange.Models;
using HippoExchange.Api.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using HippoExchange.Api.Examples;
using HippoExchange.Api.Models;
using HippoExchange.Models.Clerk;
using System.Text.Json;
using Google.Cloud.SecretManager.V1;
using Figgle;
using Figgle.Fonts;
using Cowsay;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Check for seeding command before building the application
var shouldSeed = args.Contains("seed") || args.Contains("--seed");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.WebHost.UseUrls("http://*:8080");




// If not in development, fetch secrets from Google Secret Manager
if (!builder.Environment.IsDevelopment())
{
    try
    {
        var client = SecretManagerServiceClient.Create();

        // Fetch Mongo Connection String
        var mongoSecretVersionName = new SecretVersionName("thehippoexchange-471003", "MONGO_CONNECTION_STRING", "latest");
        var mongoResult = client.AccessSecretVersion(mongoSecretVersionName);
        builder.Configuration["Mongo:ConnectionString"] = mongoResult.Payload.Data.ToStringUtf8();

        // Fetch Cloudinary URL
        var cloudinarySecretVersionName = new SecretVersionName("thehippoexchange-471003", "CLOUDINARY_URL", "latest");
        var cloudinaryResult = client.AccessSecretVersion(cloudinarySecretVersionName);
        builder.Configuration["CLOUDINARY_URL"] = cloudinaryResult.Payload.Data.ToStringUtf8();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching secrets from Google Secret Manager: {ex.Message}");
        throw;
    }
}

// Configure JSON options to handle camelCase from clients
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Bind Mongo settings from env vars or appsettings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<AssetService>();
builder.Services.AddSingleton<MaintenanceService>();
builder.Services.AddSingleton<DatabaseSeeder>();

// Bind and register Cloudinary settings from URL
var cloudinaryUrl = builder.Configuration["CLOUDINARY_URL"];
if (string.IsNullOrEmpty(cloudinaryUrl))
{
    // This will help diagnose configuration issues on startup.
    throw new ArgumentException("CLOUDINARY_URL is not configured. Please check your secrets or appsettings.json.");
}
builder.Services.AddSingleton(new Cloudinary(cloudinaryUrl));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HippoExchange API", Version = "v1" });
    c.ExampleFilters();
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Temporary User ID for authentication. Enter any string.",
        Name = "X-User-Id",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            new string[] {}
        }
    });
    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
});
builder.Services.AddSwaggerExamplesFromAssemblies(typeof(ClerkWebhookExample).Assembly);

var app = builder.Build();

// Handle database seeding command
if (shouldSeed)
{
    var seeder = app.Services.GetRequiredService<DatabaseSeeder>();
    
    try
    {
        Console.WriteLine("🌱 Seeding database with demo data...\n");
        await seeder.SeedDatabaseAsync();
        
        Console.WriteLine("\n✨ Seeding completed successfully!");
        return; // Exit after seeding
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error during seeding: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        return;
    }
}

// If we're in a container in dev, we won't have the dev cert.
// The docker-compose file sets the URL to http only, so we need to clear
// the default https endpoint to prevent Kestrel from trying to load the cert.
if (builder.Environment.IsDevelopment() && builder.Configuration["ASPNETCORE_URLS"]?.Contains("http://") == true)
{
    app.Urls.Clear();
    app.Urls.Add("http://*:8080");
}

FiggleFont font = FiggleFonts.Bulbhead;

var staticCow = await DefaultCattleFarmer.RearCowWithDefaults("default");
app.MapGet("/join", () => Results.Text(font.Render("Welcome to the bloat!")));

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// TEMP auth placeholder until Clerk: header "X-User-Id"
string? GetUserId(HttpContext ctx) =>
    ctx.Request.Headers.TryGetValue("X-User-Id", out var v) ? v.ToString() : null;

// POST /assets - Create a new asset
app.MapPost("/assets", async ([FromServices] AssetService assetService, HttpContext ctx, [FromBody] CreateAssetRequest assetRequest) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var newAsset = new Assets
    {
        OwnerUserId = userId,
        ItemName = assetRequest.ItemName,
        BrandName = assetRequest.BrandName,
        Category = assetRequest.Category,
        PurchaseDate = assetRequest.PurchaseDate,
        PurchaseCost = assetRequest.PurchaseCost,
        CurrentLocation = assetRequest.CurrentLocation,
        Images = assetRequest.Images,
        ConditionDescription = assetRequest.ConditionDescription,
        Status = assetRequest.Status,
        Favorite = assetRequest.Favorite
    };

    var createdAsset = await assetService.CreateAssetAsync(newAsset);
    return Results.Created($"/assets/{createdAsset.Id}", createdAsset);
});

// GET /assets - Get all assets for the current user
app.MapGet("/assets", async ([FromServices] AssetService assetService, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var assets = await assetService.GetAssetsByOwnerIdAsync(userId);
    return Results.Ok(assets);
});

// GET /assets/{assetId} - Get a specific asset
app.MapGet("/assets/{assetId}", async ([FromServices] AssetService assetService, HttpContext ctx, string assetId) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var asset = await assetService.GetAssetByIdAsync(assetId);
    if (asset is null) return Results.NotFound();
    if (asset.OwnerUserId != userId) return Results.Forbid();

    return Results.Ok(asset);
});

// PUT /assets/{assetId} - Update an asset
app.MapPut("/assets/{assetId}", async ([FromServices] AssetService assetService, HttpContext ctx, string assetId, [FromBody] UpdateAssetRequest updatedAssetRequest) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var existingAsset = await assetService.GetAssetByIdAsync(assetId);
    if (existingAsset is null) return Results.NotFound();
    if (existingAsset.OwnerUserId != userId) return Results.Forbid();

    var updatedAsset = new Assets
    {
        Id = assetId,
        OwnerUserId = userId,
        ItemName = updatedAssetRequest.ItemName,
        BrandName = updatedAssetRequest.BrandName,
        Category = updatedAssetRequest.Category,
        PurchaseDate = updatedAssetRequest.PurchaseDate,
        PurchaseCost = updatedAssetRequest.PurchaseCost,
        CurrentLocation = updatedAssetRequest.CurrentLocation,
        Images = updatedAssetRequest.Images,
        ConditionDescription = updatedAssetRequest.ConditionDescription,
        Status = updatedAssetRequest.Status,
        Favorite = updatedAssetRequest.Favorite
    };

    var success = await assetService.ReplaceAssetAsync(assetId, updatedAsset);
    return success ? Results.NoContent() : Results.Problem("Update failed.");
});

// DELETE /assets/{assetId} - Delete an asset
app.MapDelete("/assets/{assetId}", async ([FromServices] AssetService assetService, HttpContext ctx, string assetId) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var asset = await assetService.GetAssetByIdAsync(assetId);
    if (asset is null) return Results.NotFound();
    if (asset.OwnerUserId != userId) return Results.Forbid();

    var success = await assetService.DeleteAsset(assetId);
    return success ? Results.NoContent() : Results.Problem("Delete failed.");
});

// POST /assets/upload-image - Upload an image and get a URL
app.MapPost("/assets/upload-image", async (IFormFile file, [FromServices] Cloudinary cloudinary) =>
{
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }

    await using var stream = file.OpenReadStream();
    var uploadParams = new ImageUploadParams()
    {
        File = new FileDescription(file.FileName, stream),
        // Optional: You can add transformations or specify a folder
        // Folder = "hippo-exchange-assets"
    };

    var uploadResult = await cloudinary.UploadAsync(uploadParams);

    if (uploadResult.Error != null)
    {
        return Results.Problem($"Image upload failed: {uploadResult.Error.Message}");
    }

    return Results.Ok(new { url = uploadResult.SecureUrl.ToString() });
})
.DisableAntiforgery(); // Necessary for file uploads from non-form sources

// GET /assets/{assetId}/maintenance - Get all maintenance for one asset
app.MapGet("/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    string assetId) =>
    {
        var records = await maintenanceService.GetMaintenanceByAssetIdAsync(assetId);
        return Results.Ok(records);
    });

// GET /maintenance - Get all maintenance records for the current user
app.MapGet("/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx) =>
    {
        var userId = GetUserId(ctx);
        if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

        // Get all assets for the user
        var userAssets = await assetService.GetAssetsByOwnerIdAsync(userId);
        if (!userAssets.Any())
        {
            return Results.Ok(new List<Maintenance>()); // Return empty list if user has no assets
        }

        // Get all asset IDs, filtering out any that might be null or empty
        var assetIds = userAssets.Select(a => a.Id)
                                 .Where(id => !string.IsNullOrEmpty(id))
                                 .Select(id => id!); // Cast to non-nullable string

        // Fetch all maintenance records for those asset IDs in a single query
        var records = await maintenanceService.GetMaintenanceByAssetIdsAsync(assetIds);
        return Results.Ok(records);
    });

// POST /assets/{assetId}/maintenance - Create a new maintenance record for a specific asset
app.MapPost("/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx,
    string assetId,
    [FromBody] CreateMaintenanceRequest request) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    // Verify user owns the asset
    var asset = await assetService.GetAssetByIdAsync(assetId);
    if (asset is null) return Results.NotFound("Asset not found.");
    if (asset.OwnerUserId != userId) return Results.Forbid();

    var newRecord = new Maintenance
    {
        AssetId = assetId, // Use assetId from route
        BrandName = request.BrandName,
        ProductName = request.ProductName,
        PurchaseLocation = request.PurchaseLocation,
        CostPaid = request.CostPaid,
        MaintenanceDueDate = request.MaintenanceDueDate,
        MaintenanceTitle = request.MaintenanceTitle,
        MaintenanceDescription = request.MaintenanceDescription,
        MaintenanceStatus = request.MaintenanceStatus,
        PreserveFromPrior = request.PreserveFromPrior,
        RequiredTools = request.RequiredTools,
        ToolLocation = request.ToolLocation
    };

    var createdRecord = await maintenanceService.CreateMaintenanceAsync(newRecord);
    return Results.Created($"/maintenance/{createdRecord.Id}", createdRecord);
});

// GET /maintenance/{maintenanceId} - Get a single maintenance record
app.MapGet("/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx,
    string maintenanceId) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var record = await maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
    if (record is null) return Results.NotFound();

    // Verify user owns the asset associated with the maintenance record
    var asset = await assetService.GetAssetByIdAsync(record.AssetId);
    if (asset is null || asset.OwnerUserId != userId) return Results.Forbid();

    return Results.Ok(record);
});

// PUT /maintenance/{maintenanceId} - Update a maintenance record
app.MapPut("/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx,
    string maintenanceId,
    [FromBody] UpdateMaintenanceRequest request) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var existingRecord = await maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
    if (existingRecord is null) return Results.NotFound();

    // Verify user owns the asset
    var asset = await assetService.GetAssetByIdAsync(existingRecord.AssetId);
    if (asset is null || asset.OwnerUserId != userId) return Results.Forbid();

    // Update properties
    existingRecord.BrandName = request.BrandName;
    existingRecord.ProductName = request.ProductName;
    existingRecord.PurchaseLocation = request.PurchaseLocation;
    existingRecord.CostPaid = request.CostPaid;
    existingRecord.MaintenanceDueDate = request.MaintenanceDueDate;
    existingRecord.MaintenanceTitle = request.MaintenanceTitle;
    existingRecord.MaintenanceDescription = request.MaintenanceDescription;
    existingRecord.MaintenanceStatus = request.MaintenanceStatus;
    existingRecord.PreserveFromPrior = request.PreserveFromPrior;
    existingRecord.RequiredTools = request.RequiredTools;
    existingRecord.ToolLocation = request.ToolLocation;

    var success = await maintenanceService.UpdateMaintenanceAsync(maintenanceId, existingRecord);
    return success ? Results.NoContent() : Results.Problem("Update failed.");
});

// DELETE /maintenance/{maintenanceId} - Delete a maintenance record
app.MapDelete("/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx,
    string maintenanceId) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var record = await maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
    if (record is null) return Results.NotFound();

    // Verify user owns the asset
    var asset = await assetService.GetAssetByIdAsync(record.AssetId);
    if (asset is null || asset.OwnerUserId != userId) return Results.Forbid();

    var success = await maintenanceService.DeleteMaintenanceAsync(maintenanceId);
    return success ? Results.NoContent() : Results.Problem("Delete failed.");
});

app.MapPost("/api/webhooks/clerk", [SwaggerRequestExample(typeof(ClerkWebhookPayload), typeof(ClerkWebhookExample))] async (
    [FromServices] UserService userService,
    [FromBody] ClerkWebhookPayload payload) =>
{
    var clerkUser = payload.Data;
    if (clerkUser is null)
    {
        return Results.BadRequest("Payload data is missing.");
    }

    if (payload.Type == "user.created" || payload.Type == "user.updated")
    {
        await userService.UpsertUserAsync(clerkUser);
        return Results.Ok(new { message = "User created or updated successfully" });
    }
    else if (payload.Type == "user.deleted")
    {
        await userService.DeleteUserAsync(clerkUser.Id);
        return Results.Ok(new { message = "User deleted successfully" });
    }

    return Results.BadRequest(new { message = $"Unhandled event type: {payload.Type}" });
});

app.MapGet("/users", async ([FromServices] UserService userService) =>
{
    var users = await userService.GetAllUsersAsync();
    return Results.Ok(users);
});

app.MapGet("/users/{userId}", async ([FromServices] UserService userService, string userId) =>
{
    var user = await userService.GetByClerkIdAsync(userId);

    if (user == null)
    {
        return Results.NotFound(new { message = "User not found" });
    }

    return Results.Ok(user);
});

app.MapPatch("/users/{userId}", async ([FromServices] UserService userService, HttpContext ctx, string userId, [FromBody] ProfileUpdateRequest updateRequest) =>
{
    var authenticatedUserId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(authenticatedUserId) || authenticatedUserId != userId)
    {
        return Results.Unauthorized();
    }

    var success = await userService.UpdateUserProfileAsync(userId, updateRequest);
    if (!success)
    {
        return Results.NotFound(new { message = "User not found or profile not updated." });
    }

    return Results.Ok(new { message = "Profile updated successfully." });
});
// This is the old DELETE endpoint, which is now replaced by the webhook-based one above.
// I'm removing it to avoid confusion.
// app.MapDelete("/users/{userId}", async ([FromServices] UserService userService, string userId) =>
// {
//     await userService.DeleteUserAsync(userId);
//     return Results.NoContent();
// });

// POST /api/admin/seed - Seed the database with demo data
app.MapPost("/api/admin/seed", async ([FromServices] DatabaseSeeder seeder) =>
{
    try
    {
        await seeder.SeedDatabaseAsync();
        return Results.Ok(new 
        { 
            message = "Database seeded successfully",
            demoUsers = new[]
            {
                new { clerkId = "user_33UeIDzYloCoZABaaCR1WPmV7MT", name = "John Smith", persona = "Homeowner" },
                new { clerkId = "user_33UeKv6eNbmLb2HClHd1PN51AZ5", name = "Jane Doe", persona = "Hobbyist" },
                new { clerkId = "user_33UeOCZ7LGxjHJ8dkwnAIozslO0", name = "Bob Builder", persona = "Contractor" }
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Seeding failed"
        );
    }
})
.WithName("SeedDatabase")
.WithTags("Admin");

// DELETE /api/admin/seed - Remove only demo data
app.MapDelete("/api/admin/seed", async ([FromServices] DatabaseSeeder seeder) =>
{
    try
    {
        await seeder.ClearDemoDataAsync();
        
        return Results.Ok(new 
        { 
            message = "Demo data removed successfully",
            removedUsers = new[] { 
                "user_33UeIDzYloCoZABaaCR1WPmV7MT",  // john_smith
                "user_33UeKv6eNbmLb2HClHd1PN51AZ5",  // jane_doe
                "user_33UeOCZ7LGxjHJ8dkwnAIozslO0"   // bob_builder
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Purge failed"
        );
    }
})
.WithName("PurgeDemoData")
.WithTags("Admin");

// GET /api/admin/seed/status - Check if demo data exists
app.MapGet("/api/admin/seed/status", async ([FromServices] UserService userService) =>
{
    var demoClerkIds = new[] { 
        "user_33UeIDzYloCoZABaaCR1WPmV7MT",  // john_smith
        "user_33UeKv6eNbmLb2HClHd1PN51AZ5",  // jane_doe
        "user_33UeOCZ7LGxjHJ8dkwnAIozslO0"   // bob_builder
    };
    var users = await userService.GetAllUsersAsync();
    var demoUsers = users.Where(u => demoClerkIds.Contains(u.ClerkId)).ToList();
    
    return Results.Ok(new
    {
        hasDemoData = demoUsers.Any(),
        demoUserCount = demoUsers.Count,
        demoUsers = demoUsers.Select(u => new
        {
            clerkId = u.ClerkId,
            name = u.FullName,
            email = u.Email,
            assetCount = u.Statistics.TotalAssets
        })
    });
})
.WithName("GetSeedStatus")
.WithTags("Admin");
    

app.Run();