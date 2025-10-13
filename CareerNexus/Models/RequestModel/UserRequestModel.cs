namespace CareerNexus.Models.RequestModel
{
    public class UserRequestModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public long? RoleId { get; set; }
        public string PassswordHash { get; set; }
    }
}
