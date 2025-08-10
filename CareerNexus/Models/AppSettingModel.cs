namespace CareerNexus.Models
{
    public class AppSettingModel
    {public string DefaultConnection { get; set; }
        public string TokenDurationInMinutes { get; set; } 
        public string ValidIssuer { get; set; } 
        public string ValidAudience { get; set; }
        public string Domain { get; set; }
        public string Secret { get; set; } 
        public string ClientId { get; set; }
        public string AppURL { get; set; } 
        public string LicenseAppURL { get; set; } 
        public string LicenseAppKey { get; set; } 
        public string IManageApiUrl { get; set; }
        public string IManage_AuthorizationEndpoint { get; set; } 
        public string IManage_TokenEndpoint { get; set; }
        public string IManage_LogoutEndpoint { get; set; }
        public string IManage_RedirectUrl { get; set; }
        public string NetDocumentApiUrl { get; set; }
        public string NetDocument_AuthorizationEndpoint { get; set; } 
        public string NetDocument_TokenEndpoint { get; set; } 
        public string NetDocument_LogoutEndpoint { get; set; }
        public string NetDocument_RedirectUrl { get; set; } 
        public string XAuth { get; set; }
    }
}
