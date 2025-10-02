namespace HippoExchange.Models
{
    public class ProfileUpdateRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public Address? Address { get; set; }
    }
}
