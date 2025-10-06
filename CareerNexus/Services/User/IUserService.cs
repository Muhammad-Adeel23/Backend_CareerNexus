using CareerNexus.Models.ChangePassword;

namespace CareerNexus.Services.User
{
    public interface IUserService
    {
        Task<bool> ForgotPassword(string email);
        Task<(bool, string)> ChangePassword(ChangePassword changePassword);
    }
}
