using HippoExchange.Models;
using HippoExchange.Api.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Filters;
using HippoExchange.Api.Examples;
using HippoExchange.Api.Models;
using HippoExchange.Models.Clerk;
using HippoExchange.Api.Utilities;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.IO;
using System.Net;
using Google.Cloud.SecretManager.V1;
using Figgle;
using Figgle.Fonts;
using Cowsay;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Clerk.BackendAPI;
using Clerk.BackendAPI.Models.Components;
using Clerk.BackendAPI.Models.Operations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MongoDB.Driver;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Svix;
using Svix.Exceptions;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;


var builder = WebApplication.CreateBuilder(args);

foreach (var source in builder.Configuration.Sources.OfType<JsonConfigurationSource>())
{
    source.ReloadOnChange = false;
}

// Check for seeding command before building the application
var shouldSeed = args.Contains("seed") || args.Contains("--seed");

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

// Bind settings from configuration or secrets
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.Configure<ClerkSettings>(builder.Configuration.GetSection("Clerk"));
var clerkSettings = builder.Configuration.GetSection("Clerk").Get<ClerkSettings>() ??
    throw new InvalidOperationException("Clerk configuration is required.");
if (string.IsNullOrWhiteSpace(clerkSettings.Issuer))
{
    throw new InvalidOperationException("Clerk issuer URL is not configured.");
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("At least one allowed origin must be configured via Cors:AllowedOrigins.");
        }

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JSON options to handle camelCase from clients
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// MongoDB client registrations
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var mongoSettings = builder.Configuration.GetSection("Mongo").Get<MongoSettings>() ??
        throw new InvalidOperationException("Mongo configuration is missing.");

    if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
    {
        throw new InvalidOperationException("Mongo connection string is not configured.");
    }

    return new MongoClient(mongoSettings.ConnectionString);
});

builder.Services.AddSingleton(serviceProvider =>
{
    var mongoSettings = builder.Configuration.GetSection("Mongo").Get<MongoSettings>() ??
        throw new InvalidOperationException("Mongo configuration is missing.");

    if (string.IsNullOrWhiteSpace(mongoSettings.DatabaseName))
    {
        throw new InvalidOperationException("Mongo database name is not configured.");
    }

    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.DatabaseName);
});

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<AssetService>();
builder.Services.AddSingleton<MaintenanceService>();
builder.Services.AddSingleton<BorrowService>();
builder.Services.AddSingleton<DatabaseSeeder>();
builder.Services.AddHttpClient();

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
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Provide the Clerk-issued JWT in the Authorization header. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
});
builder.Services.AddSwaggerExamplesFromAssemblies(typeof(ClerkWebhookExample).Assembly);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = clerkSettings.Issuer;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = clerkSettings.Issuer,
        ValidateAudience = !string.IsNullOrWhiteSpace(clerkSettings.Audience),
        ValidAudience = clerkSettings.Audience,
        ValidateLifetime = true,
        NameClaimType = ClaimTypes.NameIdentifier,
    };
});

builder.Services.AddAuthorization();

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

app.UseCors("ApiCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

/*

This is the start of the API ENDPOINT Area

*/

// Extract the authenticated Clerk user identifier from the JWT claims
string? GetUserId(HttpContext ctx) =>
    ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? ctx.User.FindFirstValue("sub")
    ?? ctx.User.FindFirstValue("user_id");

// POST /assets - Create a new asset
app.MapPost("/assets", async ([FromServices] AssetService assetService, HttpContext ctx, [FromBody] CreateAssetRequest assetRequest) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    // First, validate the incoming request model.
    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(assetRequest, null, null);
    if (!Validator.TryValidateObject(assetRequest, validationContext, validationResults, true))
    {
        return Results.BadRequest(new { errors = validationResults.Select(v => v.ErrorMessage) });
    }

    // If validation passes, create the final Assets object for the database.
    var newAsset = new Assets
    {
        OwnerUserId = userId, // Link asset to the authenticated user
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

    newAsset = InputSanitizer.SanitizeObject(newAsset);

    var createdAsset = await assetService.CreateAssetAsync(newAsset);
    return Results.Created($"/assets/{createdAsset.Id}", createdAsset);
}).RequireAuthorization();

//Patch /assets/{assetId} - patches a new value for the favorite attribute for an asset 
app.MapPatch("/assets/{assetId}", async ([FromServices] AssetService assetService, HttpContext ctx, string assetId, [FromBody] bool isFavorite) =>
{
    //checks for username to see if it's a bad request
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId))
        return Results.Unauthorized();

    //This is a validation to ensure we are gettint proper data 
    if (!ctx.Request.ContentType?.Contains("application/json") ?? true)
        return Results.BadRequest(new { error = "Invalid request format." });


    var success = await assetService.UpdateFavorite(assetId, isFavorite);

    return success
        ? Results.Ok(new { message = $"Favorite status updated to {isFavorite}." })
        : Results.NotFound(new { error = "Asset not found or no changes made." });
}).RequireAuthorization();


// GET /assets - Get all assets for the current user
app.MapGet("/assets", async ([FromServices] AssetService assetService, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var scope = ctx.Request.Query["scope"].ToString();

    if (!string.IsNullOrEmpty(scope) && scope.Equals("community", StringComparison.OrdinalIgnoreCase))
    {
        var limitQuery = ctx.Request.Query["limit"].ToString();
        var limit = 100;
        if (!string.IsNullOrWhiteSpace(limitQuery) && int.TryParse(limitQuery, out var parsed))
        {
            limit = Math.Clamp(parsed, 1, 500);
        }

        var searchTerm = ctx.Request.Query["search"].ToString();
        var communityAssets = await assetService.GetCommunityAssetsAsync(userId, limit, searchTerm);
        return Results.Ok(communityAssets);
    }

    var assets = await assetService.GetAssetsByOwnerIdAsync(userId);
    return Results.Ok(assets);
}).RequireAuthorization();

// GET /assets/{assetId} - Get a specific asset
app.MapGet("/assets/{assetId}", async ([FromServices] AssetService assetService, HttpContext ctx, string assetId) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var asset = await assetService.GetAssetByIdAsync(assetId);
    if (asset is null) return Results.NotFound();
    if (asset.OwnerUserId != userId) return Results.Forbid();

    return Results.Ok(asset);
}).RequireAuthorization();

// Borrow request endpoints
app.MapPost("/borrow-requests", async (
    [FromServices] BorrowService borrowService,
    HttpContext ctx,
    [FromBody] CreateBorrowRequest request) =>
{
    var borrowerId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(borrowerId)) return Results.Unauthorized();

    var validationResults = new List<ValidationResult>();
    var validationContext = new ValidationContext(request, null, null);
    if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
    {
        return Results.BadRequest(new { errors = validationResults.Select(v => v.ErrorMessage) });
    }

    try
    {
        var created = await borrowService.CreateRequestAsync(request, borrowerId);
        return Results.Created($"/borrow-requests/{created.Id}", created);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
}).RequireAuthorization();

app.MapGet("/borrow-requests/borrower", async (
    [FromServices] BorrowService borrowService,
    HttpContext ctx) =>
{
    var borrowerId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(borrowerId)) return Results.Unauthorized();

    var summaries = await borrowService.GetBorrowerSummariesAsync(borrowerId);
    return Results.Ok(summaries);
}).RequireAuthorization();

app.MapGet("/borrow-requests/owner", async (
    [FromServices] BorrowService borrowService,
    HttpContext ctx) =>
{
    var ownerId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(ownerId)) return Results.Unauthorized();

    var summaries = await borrowService.GetOwnerSummariesAsync(ownerId);
    return Results.Ok(summaries);
}).RequireAuthorization();

app.MapPatch("/borrow-requests/{requestId}/decision", async (
    [FromServices] BorrowService borrowService,
    HttpContext ctx,
    string requestId,
    [FromBody] BorrowDecisionRequest request) =>
{
    var ownerId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(ownerId)) return Results.Unauthorized();

    try
    {
        var updated = await borrowService.DecideAsync(requestId, ownerId, request);
        if (updated is null) return Results.NotFound();
        return Results.Ok(updated);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
}).RequireAuthorization();

app.MapPatch("/borrow-requests/{requestId}/complete", async (
    [FromServices] BorrowService borrowService,
    HttpContext ctx,
    string requestId,
    [FromBody] CompleteBorrowRequest request) =>
{
    var ownerId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(ownerId)) return Results.Unauthorized();

    try
    {
        var updated = await borrowService.CompleteAsync(requestId, ownerId, request.Note);
        if (updated is null) return Results.NotFound();
        return Results.Ok(updated);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
}).RequireAuthorization();

app.MapGet("/borrow-requests/{requestId:length(24)}", async (
    [FromServices] BorrowService borrowService,
    HttpContext ctx,
    string requestId) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var borrowRequest = await borrowService.GetByIdAsync(requestId);
    if (borrowRequest is null) return Results.NotFound();

    if (borrowRequest.BorrowerUserId != userId && borrowRequest.OwnerUserId != userId)
    {
        return Results.Forbid();
    }

    return Results.Ok(borrowRequest);
}).RequireAuthorization();

// PUT /assets/{assetId} - Update an asset
app.MapPut("/assets/{assetId}", async ([FromServices] AssetService assetService, HttpContext ctx, string assetId, [FromBody] UpdateAssetRequest updatedAssetRequest) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var existingAsset = await assetService.GetAssetByIdAsync(assetId);
    if (existingAsset is null) return Results.NotFound();
    if (existingAsset.OwnerUserId != userId) return Results.Forbid();

    // Apply updates from the request to the existing asset
    if (updatedAssetRequest.ItemName is not null) existingAsset.ItemName = updatedAssetRequest.ItemName;
    if (updatedAssetRequest.BrandName is not null) existingAsset.BrandName = updatedAssetRequest.BrandName;
    if (updatedAssetRequest.Category is not null) existingAsset.Category = updatedAssetRequest.Category;
    if (updatedAssetRequest.PurchaseDate.HasValue) existingAsset.PurchaseDate = updatedAssetRequest.PurchaseDate.Value;
    if (updatedAssetRequest.PurchaseCost.HasValue) existingAsset.PurchaseCost = updatedAssetRequest.PurchaseCost.Value;
    if (updatedAssetRequest.CurrentLocation is not null) existingAsset.CurrentLocation = updatedAssetRequest.CurrentLocation;
    if (updatedAssetRequest.Images is not null) existingAsset.Images = updatedAssetRequest.Images;
    if (updatedAssetRequest.ConditionDescription is not null) existingAsset.ConditionDescription = updatedAssetRequest.ConditionDescription;
    if (updatedAssetRequest.Status.HasValue) existingAsset.Status = updatedAssetRequest.Status.Value;
    if (updatedAssetRequest.Favorite.HasValue) existingAsset.Favorite = updatedAssetRequest.Favorite.Value;

    var sanitizedAsset = InputSanitizer.SanitizeObject(existingAsset);

    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(sanitizedAsset, null, null);

    if (!Validator.TryValidateObject(sanitizedAsset, context, validationResults, true))
    {
        // Return 400 with all validation messages
        return Results.BadRequest(new
        {
            errors = validationResults.Select(v => v.ErrorMessage)
        });
    }

    var success = await assetService.ReplaceAssetAsync(assetId, sanitizedAsset);
    return success ? Results.NoContent() : Results.Problem("Update failed.");
}).RequireAuthorization();

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
}).RequireAuthorization();


//Get /assets/{assetId}/images
app.MapGet("/assets/{assetId}/images" , async ([FromServices] AssetService assetService, HttpContext ctx, string assetId) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var asset = await assetService.GetAssetByIdAsync(assetId);
    if (asset is null) return Results.NotFound(new { error = "Asset not found" });
    if (asset.OwnerUserId != userId) return Results.Forbid();

    //calls the method in assetService to get assets image 
    var images = await assetService.GetAssetImage(assetId);
    //if the assets wasn't found then should get nothing back
    if (images == null)
    {
        return Results.NotFound(new { error = "Asset not found" });
    }

    return Results.Ok(images);
}).RequireAuthorization();

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
.DisableAntiforgery()
.RequireAuthorization(); // Disable antiforgery because uploads use multipart form data
/*


This begins the area with the maintenence api endpoints 


*/
// GET /assets/{assetId}/maintenance - Get all maintenance for one asset
app.MapGet("/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx,
    string assetId) =>
    {
        var userId = GetUserId(ctx);
        if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

        var asset = await assetService.GetAssetByIdAsync(assetId);
        if (asset is null) return Results.NotFound("Asset not found.");
        if (asset.OwnerUserId != userId) return Results.Forbid();

        var records = await maintenanceService.GetMaintenanceByAssetIdAsync(assetId);
        return Results.Ok(records);
    }).RequireAuthorization();

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
    }).RequireAuthorization();

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
        CostPaid = request.CostPaid,
        MaintenanceDueDate = request.MaintenanceDueDate,
        MaintenanceTitle = request.MaintenanceTitle,
        MaintenanceDescription = request.MaintenanceDescription,
        MaintenanceStatus = request.MaintenanceStatus,
        RequiredTools = request.RequiredTools ?? new List<string>(),
        ToolLocation = request.ToolLocation,
        PreserveFromPrior = request.PreserveFromPrior,
        RecurrenceInterval = request.RecurrenceInterval,
        RecurrenceUnit = request.RecurrenceUnit
    };

    // Sanitize Data
    newRecord = InputSanitizer.SanitizeObject(newRecord);

    var createdRecord = await maintenanceService.CreateMaintenanceAsync(newRecord);
    return Results.Created($"/maintenance/{createdRecord.Id}", createdRecord);
}).RequireAuthorization();

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
}).RequireAuthorization();

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

    // Update all properties from the request
    existingRecord.BrandName = request.BrandName;
    existingRecord.ProductName = request.ProductName;
    existingRecord.AssetCategory = request.AssetCategory;
    existingRecord.CostPaid = request.CostPaid;
    existingRecord.MaintenanceDueDate = request.MaintenanceDueDate;
    existingRecord.MaintenanceTitle = request.MaintenanceTitle;
    existingRecord.MaintenanceDescription = request.MaintenanceDescription;
    existingRecord.MaintenanceStatus = request.MaintenanceStatus;
    existingRecord.IsCompleted = request.IsCompleted;
    existingRecord.RequiredTools = request.RequiredTools ?? new List<string>();
    existingRecord.ToolLocation = request.ToolLocation;
    
    // FIX: Add the missing recurrence properties
    existingRecord.PreserveFromPrior = request.PreserveFromPrior;
    existingRecord.RecurrenceInterval = request.RecurrenceInterval;
    existingRecord.RecurrenceUnit = request.RecurrenceUnit;

    // Sanitize data
    var sanitizedRecord = InputSanitizer.SanitizeObject(existingRecord);

    var success = await maintenanceService.UpdateMaintenanceAsync(maintenanceId, sanitizedRecord);
    
    return success 
        ? Results.NoContent() 
        : Results.Problem("Update failed.");
}).RequireAuthorization();

// PATCH /maintenance/{maintenanceId} - Partially update a maintenance record
app.MapPatch("/maintenance/{maintenanceId}", async (
    [FromServices] MaintenanceService maintenanceService,
    [FromServices] AssetService assetService,
    HttpContext ctx,
    string maintenanceId,
    [FromBody] PatchMaintenanceRequest request) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var existingRecord = await maintenanceService.GetMaintenanceByIdAsync(maintenanceId);
    if (existingRecord is null) return Results.NotFound();

    // Verify user owns the asset
    var asset = await assetService.GetAssetByIdAsync(existingRecord.AssetId);
    if (asset is null || asset.OwnerUserId != userId) return Results.Forbid();

    // Validate the incoming request
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(request, null, null);
    if (!Validator.TryValidateObject(request, context, validationResults, true))
    {
        return Results.BadRequest(new { errors = validationResults.Select(v => v.ErrorMessage) });
    }

    // Apply updates for provided fields
    if (request.BrandName is not null) existingRecord.BrandName = request.BrandName;
    if (request.ProductName is not null) existingRecord.ProductName = request.ProductName;
    if (request.PurchaseLocation is not null) existingRecord.PurchaseLocation = request.PurchaseLocation;
    if (request.AssetCategory is not null) existingRecord.AssetCategory = request.AssetCategory;
    if (request.CostPaid.HasValue) existingRecord.CostPaid = request.CostPaid.Value;
    if (request.MaintenanceDueDate.HasValue) existingRecord.MaintenanceDueDate = request.MaintenanceDueDate.Value;
    if (request.MaintenanceTitle is not null) existingRecord.MaintenanceTitle = request.MaintenanceTitle;
    if (request.MaintenanceDescription is not null) existingRecord.MaintenanceDescription = request.MaintenanceDescription;
    if (request.MaintenanceStatus is not null) existingRecord.MaintenanceStatus = request.MaintenanceStatus;
    if (request.IsCompleted.HasValue) existingRecord.IsCompleted = request.IsCompleted.Value;
    if (request.RequiredTools is not null) existingRecord.RequiredTools = request.RequiredTools;
    if (request.ToolLocation is not null) existingRecord.ToolLocation = request.ToolLocation;
    if (request.PreserveFromPrior.HasValue) existingRecord.PreserveFromPrior = request.PreserveFromPrior.Value;
    if (request.RecurrenceInterval.HasValue) existingRecord.RecurrenceInterval = request.RecurrenceInterval.Value;
    if (request.RecurrenceUnit.HasValue) existingRecord.RecurrenceUnit = request.RecurrenceUnit.Value;

    // Sanitize the entire object after patching
    var sanitizedRecord = InputSanitizer.SanitizeObject(existingRecord);

    var success = await maintenanceService.UpdateMaintenanceAsync(maintenanceId, sanitizedRecord);
    
    return success 
        ? Results.NoContent() 
        : Results.Problem("Update failed.");
}).RequireAuthorization();

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
}).RequireAuthorization();
/*


This begins the area with the user and unknown endpoints. 


*/
//This creates a user and gets the information needed from clerk 
app.MapPost("/api/webhooks/clerk", [SwaggerRequestExample(typeof(ClerkWebhookPayload), typeof(ClerkWebhookExample))] async (
    [FromServices] UserService userService,
    [FromServices] IOptions<ClerkSettings> clerkOptions,
    HttpContext ctx) =>
{
    var webhookSecret = clerkOptions.Value.WebhookSecret ?? Environment.GetEnvironmentVariable("CLERK_WEBHOOK_SECRET");
    if (string.IsNullOrWhiteSpace(webhookSecret))
    {
        return Results.Problem("Clerk webhook secret is not configured.", statusCode: 500);
    }

    var svixId = ctx.Request.Headers["svix-id"].FirstOrDefault();
    var svixTimestamp = ctx.Request.Headers["svix-timestamp"].FirstOrDefault();
    var svixSignature = ctx.Request.Headers["svix-signature"].FirstOrDefault();

    if (string.IsNullOrEmpty(svixId) || string.IsNullOrEmpty(svixTimestamp) || string.IsNullOrEmpty(svixSignature))
    {
        return Results.BadRequest(new { message = "Missing required Svix signature headers." });
    }

    ctx.Request.EnableBuffering();
    string rawBody;
    using (var reader = new StreamReader(ctx.Request.Body, leaveOpen: true))
    {
        rawBody = await reader.ReadToEndAsync();
        ctx.Request.Body.Position = 0;
    }

    try
    {
        var headers = new WebHeaderCollection
        {
            { "svix-id", svixId },
            { "svix-timestamp", svixTimestamp },
            { "svix-signature", svixSignature }
        };

        var webhook = new Webhook(webhookSecret);
        webhook.Verify(rawBody, headers);
    }
    catch (WebhookVerificationException)
    {
        return Results.Unauthorized();
    }

    ClerkWebhookPayload? payload;
    try
    {
        payload = JsonSerializer.Deserialize<ClerkWebhookPayload>(rawBody);
    }
    catch (JsonException)
    {
        return Results.BadRequest(new { message = "Invalid webhook payload." });
    }

    var clerkUser = payload?.Data;
    if (clerkUser is null)
    {
        return Results.BadRequest("Payload data is missing.");
    }

    if (string.IsNullOrWhiteSpace(payload?.Type))
    {
        return Results.BadRequest(new { message = "Webhook type is missing." });
    }

    switch (payload.Type)
    {
        case "user.created":
        case "user.updated":
            await userService.UpsertUserAsync(clerkUser);
            return Results.Ok(new { message = "User created or updated successfully" });
        case "user.deleted":
            await userService.DeleteUserAsync(clerkUser.Id);
            return Results.Ok(new { message = "User deleted successfully" });
        default:
            return Results.BadRequest(new { message = $"Unhandled event type: {payload.Type}" });
    }
}).AllowAnonymous();

//The reads all users (for dev purposes)
app.MapGet("/users", async ([FromServices] UserService userService, HttpContext ctx, IHostEnvironment env) =>
{
    if (!env.IsDevelopment())
    {
        return Results.NotFound();
    }

    var requesterId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(requesterId)) return Results.Unauthorized();

    var users = await userService.GetAllUsersAsync();
    return Results.Ok(users);
}).RequireAuthorization();

//This read a specific user by their userId
app.MapGet("/users/{userId}", async (
    [FromServices] UserService userService,
    HttpContext ctx,
    IHostEnvironment env,
    string userId) =>
{
    var requesterId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(requesterId)) return Results.Unauthorized();

    if (!env.IsDevelopment() && requesterId != userId)
    {
        return Results.Forbid();
    }

    var user = await userService.GetByClerkIdAsync(userId);

    if (user == null)
    {
        return Results.NotFound(new { message = "User not found" });
    }

    return Results.Ok(user);
}).RequireAuthorization();

//This is used to update a users information 
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
}).RequireAuthorization();

// A patch endpoint to allow for the app devs to talk to clerk and update user info
// IMPORTANT: Requires all fields to be included, whether or not they were changed
app.MapPatch("/update-clerk-user/{userId}", async (
    string userId,
    HttpContext ctx,
    IHostEnvironment env,
    [FromServices] IHttpClientFactory httpClientFactory,
    [FromServices] IOptions<ClerkSettings> clerkOptions,
    [FromBody] ClerkUserUpdateRequest updateRequest) =>
{
    var requesterId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(requesterId)) return Results.Unauthorized();

    if (!env.IsDevelopment())
    {
        return Results.Forbid();
    }

    var clerkSecret = clerkOptions.Value.SecretKey ?? Environment.GetEnvironmentVariable("CLERK_SECRET_KEY");
    if (string.IsNullOrWhiteSpace(clerkSecret))
    {
        return Results.Problem("Clerk secret key is not configured.", statusCode: 500);
    }

    var client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clerkSecret);

    var response = await client.PatchAsJsonAsync($"https://api.clerk.com/v1/users/{userId}", updateRequest);
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        return Results.BadRequest(new { error = content });
    }

    return Results.Ok(JsonDocument.Parse(content));
}).RequireAuthorization();

// This is the old DELETE endpoint, which is now replaced by the webhook-based one above.
// I'm removing it to avoid confusion.
// app.MapDelete("/users/{userId}", async ([FromServices] UserService userService, string userId) =>
// {
//     await userService.DeleteUserAsync(userId);
//     return Results.NoContent();
// });

// POST /api/admin/seed - Seed the database with demo data
app.MapPost("/api/admin/seed", async (
    [FromServices] DatabaseSeeder seeder,
    IHostEnvironment env) =>
{
    if (!env.IsDevelopment())
    {
        return Results.Forbid();
    }

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
app.MapDelete("/api/admin/seed", async (
    [FromServices] DatabaseSeeder seeder,
    HttpContext ctx,
    IHostEnvironment env) =>
{
    var requesterId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(requesterId)) return Results.Unauthorized();
    if (!env.IsDevelopment())
    {
        return Results.Forbid();
    }

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
}).RequireAuthorization()
.WithName("PurgeDemoData")
.WithTags("Admin");

// GET /api/admin/seed/status - Check if demo data exists
app.MapGet("/api/admin/seed/status", async (
    [FromServices] UserService userService,
    HttpContext ctx,
    IHostEnvironment env) =>
{
    var requesterId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(requesterId)) return Results.Unauthorized();
    if (!env.IsDevelopment())
    {
        return Results.Forbid();
    }

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
}).RequireAuthorization()
.WithName("GetSeedStatus")
.WithTags("Admin");
    


app.Run();
