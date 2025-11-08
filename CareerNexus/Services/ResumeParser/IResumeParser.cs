namespace CareerNexus.Services.ResumeParser
{
    public interface IResumeParser
    {
        Task<string> ExtractTextFromFileAsync(IFormFile file);
    }
}
