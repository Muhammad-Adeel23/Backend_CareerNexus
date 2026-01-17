using CareerNexus.Common;
using CareerNexus.Models;
using CareerNexus.Models.Resume;
using CareerNexus.Services.ArtificalIntelligence;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CareerNexus.Services.ResumeAnalyzer
{
    public class ResumeAnalyzer : IResumeAnalyzer
    {
        private readonly IArtificialIntelligence _aiService;
       
        public ResumeAnalyzer(IArtificialIntelligence aiService, IConfiguration config)
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
  ""CareerRecommendation"": [string],   // Recommend  career based on skills or general information of CV
  ""CareerCount"": number 
}}
Resume Text:
{resumeText}

Instructions:
- Suggest only one suitable career titles in 'CareerRecommendation' based on skills found in the resume.
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

        public async Task<Dictionary<string, List<string>>> GetTutorialLinksAsync(List<string> missingSkills)
        {
            var tutorials = new Dictionary<string, List<string>>();

            foreach (var skill in missingSkills)
            {
                var prompt = $@"List 3 free and updated online tutorials (YouTube, Coursera, or official documentation)
               for learning '{skill}'. Respond strictly in JSON array format, example: [""url1"", ""url2"", ""url3""]";

                var response = await _aiService.OpenAITurboModelAsync(prompt, skill);

                try
                {
                    var j = JToken.Parse(response);
                    var content = j.SelectToken("choices[0].message.content")?.ToString() ?? response;
                    if (content.StartsWith("```"))
                        content = content.Replace("```json", "").Replace("```", "").Trim();

                    var links = JsonConvert.DeserializeObject<List<string>>(content);
                    tutorials[skill] = links ?? new List<string>();
                }
                catch
                {
                    tutorials[skill] = new List<string>();
                }
            }

            return tutorials;
        }


        public Task<SaveResumeDTO> GetLatestResume(long userId)
        {
            string query = @"
     SELECT TOP 1 
         FileURL,
         ParsedSkills,
         Analysis,
         UploadedAt
     FROM Resumes
     WHERE UserId = @UserId
     ORDER BY UploadedAt DESC";

            SqlCommand cmd = new SqlCommand(query);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

            if (dt.Rows.Count == 0)
                return Task.FromResult<SaveResumeDTO>(null);

            var dto = new SaveResumeDTO
            {
                FileURL = dt.Rows[0]["FileURL"].ToString(),
                ParsedSkills = dt.Rows[0]["ParsedSkills"].ToString(),
                Analysis = dt.Rows[0]["Analysis"].ToString(),
                UploadedAt = Convert.ToDateTime(dt.Rows[0]["UploadedAt"])
            };

            return Task.FromResult(dto);
        }

        public ResumeAnalysisResult ParseAnalysis(string analysisJson)
        {
            if (string.IsNullOrEmpty(analysisJson))
                return null;

            return JsonConvert.DeserializeObject<ResumeAnalysisResult>(analysisJson);
        }

    }
}
    
    