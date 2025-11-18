using CareerNexus.Models.Resume;

namespace CareerNexus.Services.ResumeAnalyzer
{
    public interface IResumeAnalyzer
    {
        Task<ResumeAnalysisResult> AnalyzeResumeAsync(string resumeText);
        Task<Dictionary<string, List<string>>> GetTutorialLinksAsync(List<string> missingSkills);
            }
}
