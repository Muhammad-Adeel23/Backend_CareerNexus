namespace CareerNexus.Services.CareerRecommendation
{
    public interface ICareerRecommendationService
    {
        Task<List<string>> RecommendCareersAsync(List<string> skills);
    }
}
