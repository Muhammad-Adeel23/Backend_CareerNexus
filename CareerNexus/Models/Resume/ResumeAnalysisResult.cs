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
        public Dictionary<string, List<string>> Tutorials { get; set; } = new();
        public Dictionary<string, List<JobVacancy>> JobVacancies { get; set; } = new();

    }
    public class JobVacancy
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
        public DateTime? PostedAt { get; set; }

    }
}
