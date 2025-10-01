namespace HippoExchange.Models
{
    public class UpdateProfileRequest
    {
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

    }
}
