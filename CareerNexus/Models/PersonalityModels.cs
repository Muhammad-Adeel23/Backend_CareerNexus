namespace CareerNexus.Models
{
    public class PersonalityModels
    {
        public class PersonalityRequest
        {

            public List<int> Answers { get; set; } = new();
            public string? TempSessionId { get; set; }
        }

        public class PersonalityResult
        {
            public string PersonalityType { get; set; } = "";
            public int CareerScore { get; set; } 
            public string Description { get; set; } = "";
            public Dictionary<string, int> AxisScores { get; set; } = new(); 
            public List<string> SuggestedCareers { get; set; } = new();
        }


  




    public class QuestionItem
        {
            public int Id { get; set; }
            public string Text { get; set; } = "";
           
            public string Axis { get; set; } = "";
            public char Polarity { get; set; } 
        }
    }
}









