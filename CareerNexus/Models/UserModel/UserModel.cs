namespace CareerNexus.Models.UserModel
{
    public class UserModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
       

        public string PassswordHash { get; set; }
       
    }
}
