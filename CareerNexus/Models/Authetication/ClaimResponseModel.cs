namespace CareerNexus.Models.Authetication
{
    public class ClaimResponseModel
    {
        public string? FullName { get; set; }
        public string RoleName { get; set; }
        public long RoleId { get; set; }
        public long RoleType { get; set; }

        public string? Email { get; set; }
        public bool IsTwoFactorEnabled { get; set; }

        
        // public bool IsGoogleAuthenticatorEnabled { get; set; }
        //public string? ProfilePictureURL { get; set; }
        public string Token { get; set; }
        //public string Message { get; set; }
       // public bool IsSuccess { get; set; }
    }
}
