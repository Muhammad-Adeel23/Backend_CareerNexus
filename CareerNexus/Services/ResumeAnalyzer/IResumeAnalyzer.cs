using CareerNexus.Models;
using CareerNexus.Models.Resume;

namespace CareerNexus.Services.ResumeAnalyzer
{
    public interface IResumeAnalyzer
    {
        Task<ResumeAnalysisResult> AnalyzeResumeAsync(string resumeText);
        ResumeAnalysisResult ParseAnalysis(string analysisJson);
        Task<Dictionary<string, List<string>>> GetTutorialLinksAsync(List<string> missingSkills);
        Task<SaveResumeDTO> GetLatestResume(long userId);
            }
}
