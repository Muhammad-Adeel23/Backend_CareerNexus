namespace CareerNexus.Models.Feedback
{
    public class FeedbackItemModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string FeedbackType { get; set; } = "suggestion";
        public DateTime SubmittedAt { get; set; }

    }
}
