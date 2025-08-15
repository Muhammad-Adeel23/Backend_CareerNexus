using CareerNexus.Models.Authetication;
using CareerNexus.Models.RequestModel;
using CareerNexus.Models.UserModel;

namespace CareerNexus.Services.Authenticate
{
    public interface IAuthenticate
    {
        Task<ClaimResponseModel> Authenticate(AuthenticationRequestModel request);
        Task<long> Register(UserModel user);
       // public string GenerateToken(string userId, string userName, IConfiguration config);
    }
}
