using CareerNexus.Models.Feedback;

namespace CareerNexus.Services.Feedback
{
    public interface IFeedbackService
    {
        Task<long> SubmitFeedbackAsync(long userId, string message, string feedbackType);
        Task<List<FeedbackItemModel>> GetAllFeedbackAsync();
        Task<List<FeedbackItemModel>> GetFeedbackByUserIdAsync(long userId);
        Task<bool> DeleteFeedbackAsync(long feedbackId, long userId);

    }
}
