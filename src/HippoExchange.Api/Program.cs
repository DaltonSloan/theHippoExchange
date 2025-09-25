using HippoExchange.Models;
using HippoExchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
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
builder.Services.Configure<HippoExchange.Models.MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<ProfileService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<AssetService>(); 

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

app.UseCors();

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


app.MapPost("/api/webhooks/clerk", [SwaggerRequestExample(typeof(ClerkWebhookPayload), typeof(ClerkWebhookExample))] async (
    [FromServices] UserService userService,
    [FromServices] EmailService emailService,
    [FromBody] ClerkWebhookPayload payload) =>
{
    if (payload.Type == "user.created" || payload.Type == "user.updated")
    {
        var clerkUser = payload.Data;
        await userService.UpsertUserAsync(clerkUser);

        if (clerkUser.EmailAddresses is not null)
        {
            foreach (var emailData in clerkUser.EmailAddresses)
            {
                await emailService.UpsertEmailAsync(emailData);
            }
        }

        return Results.Ok(new { message = "Webhook processed successfully" });
    }

    return Results.BadRequest("Unhandled webhook type");
});

app.MapGet("/api/users", async ([FromServices] UserService userService) =>
{
    var users = await userService.GetAllUsersAsync();
    return Results.Ok(users);
});

app.MapGet("/api/user", async ([FromServices] UserService userService, HttpContext ctx) =>
{
    var userId = GetUserId(ctx);
    if (string.IsNullOrWhiteSpace(userId)) return Results.Unauthorized();

    var user = await userService.GetByClerkIdAsync(userId);

    if (user == null)
    {
        return Results.NotFound(new { message = "User not found" });
    }

    return Results.Ok(user);
});

app.Run();