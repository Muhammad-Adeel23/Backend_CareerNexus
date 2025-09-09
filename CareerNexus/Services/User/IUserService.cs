namespace CareerNexus.Services.User
{
    public interface IUserService
    {
        Task<bool> ForgotPassword(string email);
    }
}
