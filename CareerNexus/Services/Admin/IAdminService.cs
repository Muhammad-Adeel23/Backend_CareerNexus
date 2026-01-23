namespace CareerNexus.Services.Admin
{
    public interface IAdminService
    {

        Task<OverviewStatsModel> GetOverviewStatsAsync();
        Task<List<UserListModel>> GetUsersAsync();
        Task<UserDetailModel?> GetUserAsync(int userId);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
        Task<bool> DeleteUserAsync(int userId);
        Task<List<AssessmentListModel>> GetAssessmentsAsync();
        Task<AssessmentDetailModel?> GetAssessmentAsync(int assessmentId);
        Task<List<ResumeListModel>> GetResumesAsync();
        Task<List<CareerModel>> GetCareersAsync();
        Task<CareerModel?> CreateCareerAsync(CareerModel career);
        Task<bool> UpdateCareerAsync(int id, CareerModel career);
        Task<bool> DeleteCareerAsync(int id);
        Task<List<SkillModel>> GetSkillsAsync();
        Task<SkillModel?> CreateSkillAsync(SkillModel skill);
        Task<bool> UpdateSkillAsync(int id, SkillModel skill);
        Task<bool> DeleteSkillAsync(int id);
        Task<Dictionary<string, string>> GetSettingsAsync();
        Task<bool> UpdateSettingsAsync(Dictionary<string, string> settings);
        Task<List<TopCareerModel>> GetTopCareersAsync();

    }
}
