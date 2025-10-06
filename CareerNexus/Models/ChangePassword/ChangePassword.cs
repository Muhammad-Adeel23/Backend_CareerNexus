namespace CareerNexus.Models.ChangePassword
{
    public class ChangePassword
    {
        public long UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
