using CareerNexus.Models.Resume;

namespace CareerNexus.Services.ResumeAnalyzer
{
    public interface IResumeAnalyzer
    {
        Task<ResumeAnalysisResult> AnalyzeResumeAsync(string resumeText);
    }
}
