using CareerNexus.Models.Authetication;
using CareerNexus.Models.UserModel;

namespace CareerNexus.Services.OtpService
{
    public interface IOTP
    {
        string BuildToken(long id, string fullName,long RoleId,string RoleName);
        Task<ClaimResponseModel> GenerateToken(UserModel user);
         //Task<ClaimResponseModel> GenerateToken(UserModel user);
        //public string GenerateToken(string userId, string userName, IConfiguration config);
    }
}
