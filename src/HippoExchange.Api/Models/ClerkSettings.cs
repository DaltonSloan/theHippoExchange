namespace HippoExchange.Api.Models
{
    public class ClerkSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string? Audience { get; set; }
        public string? SecretKey { get; set; }
        public string? WebhookSecret { get; set; }
    }
}
