using CareerNexus.Models.RequestModel;
using CareerNexus.Models.UserModel;

namespace CareerNexus.Services.Authenticate
{
    public interface IAuthenticate
    {
        Task<ClaimResponseModel> Authenticate(AuthenticationRequestModel request);
         UserModel Signup(UserModel user);
       // public string GenerateToken(string userId, string userName, IConfiguration config);
    }
}
