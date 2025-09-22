using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace HippoExchange.Models.Clerk
{
    public class ClerkWebhookPayload
    {
        [JsonPropertyName("data")]
        public ClerkUserData Data { get; set; } = new();

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class ClerkUserData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("has_image")]
        public bool HasImage { get; set; }

        [JsonPropertyName("primary_email_address_id")]
        public string? PrimaryEmailAddressId { get; set; }

        [JsonPropertyName("primary_phone_number_id")]
        public string? PrimaryPhoneNumberId { get; set; }

        [JsonPropertyName("primary_web3_wallet_id")]
        public string? PrimaryWeb3WalletId { get; set; }

        [JsonPropertyName("password_enabled")]
        public bool PasswordEnabled { get; set; }

        [JsonPropertyName("two_factor_enabled")]
        public bool TwoFactorEnabled { get; set; }

        [JsonPropertyName("email_addresses")]
        public List<ClerkEmailAddress> EmailAddresses { get; set; } = new();

        [JsonPropertyName("phone_numbers")]
        public List<object> PhoneNumbers { get; set; } = new();

        [JsonPropertyName("external_accounts")]
        public List<object> ExternalAccounts { get; set; } = new();

        [JsonPropertyName("public_metadata")]
        public object PublicMetadata { get; set; } = new();

        [JsonPropertyName("private_metadata")]
        public object PrivateMetadata { get; set; } = new();

        [JsonPropertyName("unsafe_metadata")]
        public object UnsafeMetadata { get; set; } = new();

        [JsonPropertyName("external_id")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("last_sign_in_at")]
        public long? LastSignInAt { get; set; }

        [JsonPropertyName("banned")]
        public bool Banned { get; set; }

        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long UpdatedAt { get; set; }
    }

    public class ClerkEmailAddress
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("email_address")]
        public string EmailAddress { get; set; } = default!;

        [JsonPropertyName("reserved")]
        public bool Reserved { get; set; }

        [JsonPropertyName("verification")]
        public ClerkVerification? Verification { get; set; }
    }

    public class ClerkVerification
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("strategy")]
        public string Strategy { get; set; } = string.Empty;
    }
}
