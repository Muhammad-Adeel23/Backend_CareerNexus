namespace CareerNexus.Models.Resume
{
    public class ResumeAnalysisResult
    {
        public int MatchPercentage { get; set; }
        public string Experience { get; set; } = string.Empty;
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public List<string> CareerRecommendation { get; set; } = new();
        public int CareerCount { get; set; } = new();
    }
}
