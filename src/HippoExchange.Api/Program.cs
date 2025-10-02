using HippoExchange.Models;
using HippoExchange.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using HippoExchange.Api.Examples;
using HippoExchange.Api.Models;
using HippoExchange.Models.Clerk;
using System.Text.Json;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Options;
using Figgle;
using Figgle.Fonts;
using Cowsay;

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


// If not in development, fetch the connection string from Google Secret Manager
if (!builder.Environment.IsDevelopment())
{
    try
    {
        const string projectId = "thehippoexchange-471003";
        const string secretId = "MONGO_CONNECTION_STRING";
        const string secretVersion = "latest";

        var client = SecretManagerServiceClient.Create();
        var secretVersionName = new SecretVersionName(projectId, secretId, secretVersion);
        var result = client.AccessSecretVersion(secretVersionName);
        var connectionString = result.Payload.Data.ToStringUtf8();

        builder.Configuration["Mongo:ConnectionString"] = connectionString;
    }
    catch (Exception ex)
    {
        // Log the exception and rethrow it to ensure the application fails to start.
        Console.WriteLine($"Error fetching secret from Google Secret Manager: {ex.Message}");
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

// GET /assets/{assetId}/maintenance - Get all maintenance for one asset
app.MapGet("/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    string assetId) =>
    {
        var records = await maintenanceService.GetMaintenanceByAssetIdAsync(assetId);
        return Results.Ok(records);
    });

// GET /maintenance - Get all maintenance records
app.MapGet("/maintenace", async (
    [FromServices] MaintenanceService maintenanceService) =>
    {
        var records = await maintenanceService.GetAllMaintenanceAsync();
        return Results.Ok(records);
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