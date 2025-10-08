namespace CareerNexus.Models.UserModel
{
    public class UserModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public long Roletype { get; set; }
        public string PassswordHash { get; set; }
       
    }
}
