using CareerNexus.Models.Resume;

namespace CareerNexus.Services.ArtificalIntelligence
{
    public interface IArtificialIntelligence
    {
        Task<string> OpenAITurboModelAsync(string prompt, string resumeText);
        Task<Dictionary<string, List<JobVacancy>>> GetJobVacanciesAsync(List<string> careers, string city);


    }
}
