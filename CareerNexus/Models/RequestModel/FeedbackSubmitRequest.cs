namespace CareerNexus.Models.RequestModel
{
    public class FeedbackSubmitRequest
    {
        public string Message { get; set; } = string.Empty;
        public string FeedbackType { get; set; } = "suggestion"; // "suggestion" | "error"

    }
}
