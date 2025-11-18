using static CareerNexus.Models.PersonalityModels;

namespace CareerNexus.Services.Personality
{
    public interface IPersonalityService
    {
        Task<PersonalityResult> AnalyzeAsync(PersonalityRequest request, bool useAiExplanation = false);

        Task<bool> SaveAssessmentResultAsync(long? userId, Guid? tempSessionId, PersonalityRequest request, PersonalityResult result);



}
}
