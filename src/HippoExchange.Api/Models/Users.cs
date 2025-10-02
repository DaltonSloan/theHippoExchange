using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using HippoExchange.Models.Clerk;
using System.Collections.Generic;

namespace HippoExchange.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("clerk_id")]
        public string ClerkId { get; set; } = default!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string? ProfileImageUrl { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("contact_information")]
        public ContactInformation ContactInformation { get; set; } = new();

        [JsonPropertyName("account_status")]
        public AccountStatus AccountStatus { get; set; } = new();

        [JsonPropertyName("statistics")]
        public Statistics Statistics { get; set; } = new();

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("has_image")]
        public bool HasImage { get; set; }

        [JsonPropertyName("primary_email_address_id")]
        public string? PrimaryEmailAddressId { get; set; }

        [JsonPropertyName("last_sign_in_at")]
        public long? LastSignInAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("address")]
        public Address? Address { get; set; }

        [JsonPropertyName("email_addresses")]
        public List<ClerkEmailAddress>? EmailAddresses { get; set; }

        [JsonPropertyName("assets")]//list of assets that the user will have 
        public List<Asset> Assets { get; set; } = new();

    }

    public class ContactInformation
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("preferred_contact_method")]
        public string PreferredContactMethod { get; set; } = "email";
    }

    public class AccountStatus
    {
        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("account_active")]
        public bool AccountActive { get; set; } = true;

        [JsonPropertyName("banned")]
        public bool Banned { get; set; }

        [JsonPropertyName("locked")]
        public bool Locked { get; set; }
    }

    public class Statistics
    {
        [JsonPropertyName("total_assets")]
        public int TotalAssets { get; set; } = 0;

        [JsonPropertyName("assets_loaned")]
        public int AssetsLoaned { get; set; } = 0;

        [JsonPropertyName("assets_borrowed")]
        public int AssetsBorrowed { get; set; } = 0;
    }

/*
    public class Assets //This is a class for assets and all things related to them
    {
        [JsonPropertyName("assets_name")]//the name of the asset
        public string AssetsName { get; set; } = "";

        [JsonPropertyName("assets_description")]//description of the asset can be anything 
        public string AssetsDescription { get; set; } = "";

        [JsonPropertyName("assets_store")]//this attribute will store where the own originally bought the asset from
        public string AssetsStore { get; set; } = "";

        [JsonPropertyName("assets_Brand")]//ex: husky, milwaky, john deer ... brands of assets 
        public string AssetsBrand { get; set; } = "";

        [JsonPropertyName("assets_purchaseCost")]//what is cost the owner to buy the asset
        public string AssetsPurchaseCost { get; set; } = "";

        [JsonPropertyName("maitenance_dueDate")]// an input date when the owner expects maintenance to next be preformed
        public string MaitenanceDueDate { get; set; } = "";

        [JsonPropertyName("maitenance_title")]//brief description of what the maintenance is ex: oil change
        public string MaitenanceTitle { get; set; } = "";

        [JsonPropertyName("maitenance_description")]//an in depth description of how to do a given maitenance
        public string MaitenanceDescription { get; set; } = "";

        [JsonPropertyName("maitenance_status")]//how much more time till maitenance needs to be preformed 
        public string MaitenanceStatus { get; set; } = "";

        [JsonPropertyName("maitenance_toolsNeeded")]//tools that may be needed to perform maintenance on asset
        public string MaitenanceToolsNeeded { get; set; } = "";

        [JsonPropertyName("maitenance_history")]//history of previous maintenance
        public string MaitenanceHistory { get; set; } = "";

    }
*/
}