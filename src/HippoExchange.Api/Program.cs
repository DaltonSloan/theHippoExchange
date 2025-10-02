using HippoExchange.Models;
using HippoExchange.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using HippoExchange.Api.Examples;
using HippoExchange.Models.Clerk;
using System.Text.Json;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Options;
using Figgle;
using Figgle.Fonts;
using Cowsay;

var builder = WebApplication.CreateBuilder(args);

// Check for seeding commands before building the application
var shouldSeed = args.Contains("seed") || args.Contains("--seed");
var shouldReset = args.Contains("reset") || args.Contains("--reset");

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

// Handle database seeding commands (only in development for safety)
if (builder.Environment.IsDevelopment() && (shouldSeed || shouldReset))
{
    var seeder = app.Services.GetRequiredService<DatabaseSeeder>();
    
    try
    {
        if (shouldReset)
        {
            Console.WriteLine("🔄 Resetting database and seeding with demo data...\n");
            await seeder.ResetDatabaseAsync();
        }
        else if (shouldSeed)
        {
            Console.WriteLine("🌱 Seeding database with demo data...\n");
            await seeder.SeedDatabaseAsync();
        }
        
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
else if (!builder.Environment.IsDevelopment() && (shouldSeed || shouldReset))
{
    Console.WriteLine("❌ Error: Database seeding is only available in Development environment for safety.");
    Console.WriteLine("   To seed the database, ensure ASPNETCORE_ENVIRONMENT=Development");
    return;
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

// POST /api/assets - Add a new asset
app.MapPost("/api/assets", async ([FromServices] AssetService assetService, HttpContext ctx, [FromBody] Assets newAsset) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    newAsset.OwnerUserId = userId;
    var createdAsset = await assetService.CreateAssetAsync(newAsset);

    return Results.Created($"/api/assets/{createdAsset.Id}", createdAsset);
});

// GET /api/assets - Get all assets for the current user
app.MapGet("/api/assets", async ([FromServices] AssetService assetService, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var assets = await assetService.GetAssetsByOwnerIdAsync(userId);
    return Results.Ok(assets);
});

// GET /api/users/{userId}/assets - Get all assets for a specific user
app.MapGet("/api/users/{userId}/assets", async ([FromServices] AssetService assetService, string userId) =>
{
    if (string.IsNullOrWhiteSpace(userId)) return Results.BadRequest("User ID cannot be empty.");

    var assets = await assetService.GetAssetsByOwnerIdAsync(userId);
    return Results.Ok(assets);
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

// PUT /api/assets/{assetId} - Replace (update) an asset
app.MapPut("/api/assets/{assetId}", async ([FromServices] AssetService assetService, string assetId, Assets updatedAsset) =>
{
    if (string.IsNullOrWhiteSpace(assetId))
        return Results.BadRequest("Asset ID cannot be empty.");

    //Ensure the asset actually exists before replacing
    var existing = await assetService.GetAssetByIdAsync(assetId);
    if (existing is null)
        return Results.NotFound($"Asset with ID {assetId} not found.");

    var success = await assetService.ReplaceAssetAsync(assetId, updatedAsset);
    if (!success)
        return Results.Problem("Failed to update asset.");

    //retun the updated asset
    return Results.Ok(updatedAsset);
});

// POST /api/assets/{assetId}/maintenance - Add maintenance to an asset
app.MapPost("/api/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    string assetId,
    Maintenance maintenance) =>
    {
        if (string.IsNullOrWhiteSpace(assetId)) return Results.BadRequest("Asset ID required");

        maintenance.AssetId = assetId;
        var created = await maintenanceService.CreateMaintenanceAsync(maintenance);
        return Results.Created($"/api/maintenance/{created.Id}", created);
    });

// GET /api/assets/{assetId}/maintenance - Get all maintenance for one asset
app.MapGet("/api/assets/{assetId}/maintenance", async (
    [FromServices] MaintenanceService maintenanceService,
    string assetId) =>
    {
        var records = await maintenanceService.GetMaintenanceByAssetIdAsync(assetId);
        return Results.Ok(records);
    });

// GET /api/maintenance - Get all maintenance records
app.MapGet("/api/maintenace", async (
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

// This is the old DELETE endpoint, which is now replaced by the webhook-based one above.
// I'm removing it to avoid confusion.
// app.MapDelete("/users/{userId}", async ([FromServices] UserService userService, string userId) =>
// {
//     await userService.DeleteUserAsync(userId);
//     return Results.NoContent();
// });
    

app.Run();