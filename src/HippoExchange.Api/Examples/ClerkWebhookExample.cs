using Swashbuckle.AspNetCore.Filters;
using HippoExchange.Models.Clerk;
using System.Collections.Generic;

namespace HippoExchange.Api.Examples
{
    public class ClerkWebhookExample : IExamplesProvider<ClerkWebhookPayload>
    {
        public ClerkWebhookPayload GetExamples()
        {
            return new ClerkWebhookPayload
            {
                Type = "user.created",
                Object = "event",
                Data = new ClerkUserData
                {
                    Id = "user_123456789",
                    Object = "user",
                    Username = "genericuser",
                    FirstName = "John",
                    LastName = "Doe",
                    ImageUrl = "https://www.example.com/image.png",
                    HasImage = true,
                    PrimaryEmailAddressId = "idn_987654321",
                    PrimaryPhoneNumberId = null,
                    PrimaryWeb3WalletId = null,
                    PasswordEnabled = true,
                    TwoFactorEnabled = false,
                    EmailAddresses = new List<ClerkEmailAddress>
                    {
                        new ClerkEmailAddress
                        {
                            Id = "idn_987654321",
                            Object = "email_address",
                            EmailAddress = "john.doe@example.com",
                            Reserved = false,
                            Verification = new ClerkVerification
                            {
                                Status = "verified",
                                Strategy = "email_code"
                            }
                        }
                    },
                    PhoneNumbers = new List<object>(),
                    ExternalAccounts = new List<object>(),
                    PublicMetadata = System.Text.Json.JsonSerializer.SerializeToElement(new {}),
                    PrivateMetadata = System.Text.Json.JsonSerializer.SerializeToElement(new {}),
                    UnsafeMetadata = System.Text.Json.JsonSerializer.SerializeToElement(new 
                    {
                        phoneNumber = "+1234567890",
                        address = new
                        {
                            street = "123 Main St",
                            city = "Springfield",
                            state = "IL",
                            postal_code = "62701",
                            country = "USA"
                        }
                    }),
                    ExternalId = null,
                    LastSignInAt = 1678886400000,
                    Banned = false,
                    CreatedAt = 1726942676273,
                    UpdatedAt = 1678886400000
                }
            };
        }
    }
}
