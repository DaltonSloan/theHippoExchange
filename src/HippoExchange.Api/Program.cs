using HippoExchange.Models;
using HippoExchange.Models.Clerk;
using HippoExchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Cowsay;
using Figgle;
using Figgle.Fonts;
using Google.Cloud.SecretManager.V1;
using System.Text.Json;
using Swashbuckle.AspNetCore.Filters;
using HippoExchange.Api.Examples;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.Configure<HippoExchange.Models.MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<ProfileService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<AssetService>();
builder.Services.AddSingleton<EditAssetService>(); 

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

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// TEMP auth placeholder until Clerk: header "X-User-Id"
string? GetUserId(HttpContext ctx) =>
    ctx.Request.Headers.TryGetValue("X-User-Id", out var v) ? v.ToString() : null;

// GET current user's profile
app.MapGet("/api/profile", async ([FromServices] ProfileService svc, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var profile = await svc.GetByUserIdAsync(userId);
    return Results.Ok(profile ?? new PersonalProfile { UserId = userId });
});

// POST create/update profile
app.MapPost("/api/profile", async ([FromServices] ProfileService svc, HttpContext ctx, [FromBody] UpdateProfileRequest incoming) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var profile = await svc.GetByUserIdAsync(userId) ?? new PersonalProfile { UserId = userId };

    profile.FullName = incoming.FullName;
    profile.Email = incoming.Email;
    profile.Phone = incoming.Phone;
    profile.Address = incoming.Address;

    await svc.UpsertAsync(profile);

    // After upserting, fetch the profile again to get the database-generated ID
    var updatedProfile = await svc.GetByUserIdAsync(userId);
    
    return Results.Ok(updatedProfile);
});

// POST /api/assets - Add a new asset
app.MapPost("/api/assets", async ([FromServices] AssetService assetService, HttpContext ctx, [FromBody] Asset newAsset) =>
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

// PUT /api/assets/{assetId} - Replace (update) an asset
app.MapPut("/api/assets/{assetId}", async ([FromServices] EditAssetService editAssetService, string assetId, Asset updatedAsset) =>
{
    if (string.IsNullOrWhiteSpace(assetId))
        return Results.BadRequest("Asset ID cannot be empty.");

    //Ensure the asset actually exists before replacing
    var existing = await editAssetService.GetAssetByIdAsync(assetId);
    if (existing is null)
        return Results.NotFound($"Asset with ID {assetId} not found.");

    var success = await editAssetService.ReplaceAssetAsync(assetId, updatedAsset);
    if (!success)
        return Results.Problem("Failed to update asset.");

    //retun the updated asset
    return Results.Ok(updatedAsset);
});



app.MapPost("/api/webhooks/clerk", [SwaggerRequestExample(typeof(ClerkWebhookPayload), typeof(ClerkWebhookExample))] async (
    [FromServices] UserService userService,
    [FromServices] EmailService emailService,
    [FromBody] ClerkWebhookPayload payload) =>
{
    if (payload.Type == "user.created" || payload.Type == "user.updated")
    {
        var clerkUser = payload.Data;
        var primaryEmail = clerkUser.EmailAddresses.FirstOrDefault(e => e.Id == clerkUser.PrimaryEmailAddressId);

        if (primaryEmail == null)
        {
            // Or handle this error as you see fit
            return Results.BadRequest("Primary email not found for user.");
        }

        var user = new User
        {
            ClerkId = clerkUser.Id,
            Email = primaryEmail.EmailAddress,
            Username = clerkUser.Username,
            FirstName = clerkUser.FirstName,
            LastName = clerkUser.LastName,
            FullName = $"{clerkUser.FirstName} {clerkUser.LastName}".Trim(),
            ProfileImageUrl = clerkUser.ImageUrl,
            ContactInformation = new ContactInformation
            {
                Email = primaryEmail.EmailAddress
            },
            AccountStatus = new AccountStatus
            {
                EmailVerified = primaryEmail.Verification?.Status == "verified",
                Banned = clerkUser.Banned,
                Locked = false // Assuming 'locked' is not a direct field from Clerk and defaulting it
            },
            Statistics = new Statistics() // All stats default to 0
        };

        await userService.UpsertUserAsync(user);

        // You can still keep the email service logic if you need a separate 'emails' collection
        foreach (var emailData in clerkUser.EmailAddresses)
        {
            var newEmail = new Email
            {
                ClerkUserId = clerkUser.Id,
                ClerkEmailId = emailData.Id,
                EmailAddress = emailData.EmailAddress,
                Reserved = emailData.Reserved
            };
            await emailService.UpsertEmailAsync(newEmail);
        }
    }

    return Results.Ok();
});

app.Run();