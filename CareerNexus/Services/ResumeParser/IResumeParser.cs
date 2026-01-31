using CareerNexus.Models.Resume;

namespace CareerNexus.Services.ResumeParser
{
    public interface IResumeParser
    {
        Task<string> ExtractTextFromFileAsync(IFormFile file);
        bool MigrateUserData(MigrateGuestDataRequest request,long UserId);
    }
}
