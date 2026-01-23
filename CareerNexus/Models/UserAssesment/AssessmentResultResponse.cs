namespace CareerNexus.Models.UserAssesment
{
    public class AssessmentResultResponse
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string PersonalityType { get; set; }
        public int CareerScore { get; set; }
        public string Description { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}
