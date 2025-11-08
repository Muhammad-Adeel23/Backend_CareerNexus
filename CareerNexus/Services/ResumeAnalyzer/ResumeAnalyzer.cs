using CareerNexus.Models.Resume;
using CareerNexus.Services.ArtificalIntelligence;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CareerNexus.Services.ResumeAnalyzer
{
    public class ResumeAnalyzer : IResumeAnalyzer
    {
        private readonly IArtificialIntelligence _aiService;
        public ResumeAnalyzer(IArtificialIntelligence aiService)
        {
            _aiService = aiService;
        }
        public async Task<ResumeAnalysisResult> AnalyzeResumeAsync(string resumeText)
        {
            var prompt = $@"
You are an expert career advisor. 
Analyze the resume below and respond ONLY with pure JSON (no markdown, no backticks). 
Use this schema:
{{
  ""MatchPercentage"": number,
  ""Experience"": string,
  ""MatchedSkills"": [string],
  ""MissingSkills"": [string],
  ""Suggestions"": [string],
  ""CareerRecommendation"": [string],   // Recommend 3-5 careers based on skills or general information of CV
  ""CareerCount"": number
}}
Resume Text:
{resumeText}

Instructions:
- Suggest 3 to 5 suitable career titles in 'CareerRecommendation' based on skills found in the resume.
- Count them and set 'CareerCount'.
- Do NOT use data from your DB; rely on skills in resume and general career knowledge.
";

            var response = await _aiService.OpenAITurboModelAsync(prompt, resumeText);

            try
            {
                // Step 1: Parse the raw response
                var j = JToken.Parse(response);

                // Step 2: Try to locate model output
                var content = j.SelectToken("choices[0].message.content")?.ToString()
                           ?? j.SelectToken("choices[0].text")?.ToString()
                           ?? response;

                // Step 3: Clean content if it contains markdown JSON block
                content = content.Trim();
                if (content.StartsWith("```"))
                {
                    // Remove markdown fences like ```json and ```
                    content = content.Replace("```json", "").Replace("```", "").Trim();
                }

                // Step 4: Deserialize to your result model
                return JsonConvert.DeserializeObject<ResumeAnalysisResult>(content)
                       ?? new ResumeAnalysisResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Resume analysis parse error: {ex.Message}\nResponse:\n{response}");
                return new ResumeAnalysisResult();
            }
        }

    }
}
    
    