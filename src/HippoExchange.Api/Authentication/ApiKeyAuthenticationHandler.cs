using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HippoExchange.Api.Models;
using HippoExchange.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HippoExchange.Api.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IOptionsMonitor<AuthSettings> _authSettings;
        private readonly UserService _userService;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthSettings> authSettings,
            UserService userService,
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            _authSettings = authSettings;
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.ApiKeyHeaderName, out var providedApiKeyValues))
            {
                return AuthenticateResult.Fail("Missing API key header.");
            }

            var providedApiKey = providedApiKeyValues.ToString();
            var configuredApiKey = _authSettings.CurrentValue.ApiKey;

            if (string.IsNullOrWhiteSpace(configuredApiKey))
            {
                return AuthenticateResult.Fail("API key is not configured.");
            }

            if (!SecureEquals(providedApiKey, configuredApiKey))
            {
                return AuthenticateResult.Fail("Invalid API key.");
            }

            if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.UserIdHeaderName, out var userIdValues))
            {
                return AuthenticateResult.Fail("Missing user header.");
            }

            var userId = userIdValues.ToString();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AuthenticateResult.Fail("Missing user header.");
            }

            var user = await _userService.GetByClerkIdAsync(userId);
            if (user is null)
            {
                return AuthenticateResult.Fail("User not found.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ClerkId)
            };

            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
            }
            else if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            }
            else if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Email));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private static bool SecureEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);

            if (leftBytes.Length != rightBytes.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
    }
}
