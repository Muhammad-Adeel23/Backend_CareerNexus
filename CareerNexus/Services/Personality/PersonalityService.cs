using CareerNexus.Common;
using CareerNexus.Services.ArtificalIntelligence;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text.Json.Serialization;
using static CareerNexus.Models.PersonalityModels;

namespace CareerNexus.Services.Personality
{
    public class PersonalityService:IPersonalityService
    {
        private readonly IArtificialIntelligence _AI;
        public PersonalityService(IArtificialIntelligence AI)
        {
            _AI = AI;
        }
        
        private static readonly List<QuestionItem> Questions = new()
        {
            
            new() { Id = 1, Axis = "EI", Polarity = 'L', Text = "I enjoy social gatherings and meet new people easily."},        // L -> E
            new() { Id = 2, Axis = "EI", Polarity = 'R', Text = "I prefer to spend time alone to recharge."},                // R -> I
            new() { Id = 3, Axis = "EI", Polarity = 'L', Text = "I feel energized when I talk to many people."},
            new() { Id = 4, Axis = "EI", Polarity = 'R', Text = "I often reflect and think privately rather than talk."},
            new() { Id = 5, Axis = "EI", Polarity = 'L', Text = "I enjoy being the center of attention in groups."},

           
            new() { Id = 6, Axis = "SN", Polarity = 'L', Text = "I focus on facts and present realities."},               // L -> S
            new() { Id = 7, Axis = "SN", Polarity = 'R', Text = "I often think about future possibilities rather than details."}, // R -> N
            new() { Id = 8, Axis = "SN", Polarity = 'L', Text = "I trust practical experience more than theories."},
            new() { Id = 9, Axis = "SN", Polarity = 'R', Text = "I enjoy imagining different possibilities and abstract ideas."},
            new() { Id = 10, Axis = "SN", Polarity = 'L', Text = "I prefer concrete instructions and step-by-step tasks."},

            
            new() { Id = 11, Axis = "TF", Polarity = 'L', Text = "I make decisions primarily using logic and objective analysis."}, // L -> T
            new() { Id = 12, Axis = "TF", Polarity = 'R', Text = "I consider people's feelings when making decisions."},          // R -> F
            new() { Id = 13, Axis = "TF", Polarity = 'L', Text = "I prefer fairness and clear rules over personal concerns."},
            new() { Id = 14, Axis = "TF", Polarity = 'R', Text = "I often think about how actions affect others emotionally."},
            new() { Id = 15, Axis = "TF", Polarity = 'L', Text = "I feel comfortable being direct and critical when needed."},

            
            new() { Id = 16, Axis = "JP", Polarity = 'L', Text = "I like having plans and following a schedule."}, // L -> J
            new() { Id = 17, Axis = "JP", Polarity = 'R', Text = "I prefer to stay open and adapt rather than stick to plans."}, // R -> P
            new() { Id = 18, Axis = "JP", Polarity = 'L', Text = "I finish tasks before starting new ones."},
            new() { Id = 19, Axis = "JP", Polarity = 'R', Text = "I enjoy last-minute changes and spontaneity."},
            new() { Id = 20, Axis = "JP", Polarity = 'L', Text = "I prefer decisions that provide closure."}
        };

        private static readonly Dictionary<string, string> Descriptions = new()
        {
            {"INTJ","Strategic, analytical, independent thinkers. Good at long-term planning and solving complex problems."},
            {"INTP","Curious, theoretical, and inventive. Enjoy exploring ideas and systems."},
            {"ENTJ","Natural leaders. Strategic, decisive, and organized—seek to drive projects forward."},
            {"ENTP","Inventive and energetic. Enjoy debate and coming up with new approaches."},
            {"INFJ","Insightful and principled. Value meaning, integrity and helping others."},
            {"INFP","Idealistic and compassionate. Guided by values and personal meaning."},
            {"ENFJ","Warm, empathetic leaders. Skilled at reading people and organizing groups."},
            {"ENFP","Enthusiastic, creative, and people-focused. Seek novelty and meaningful connections."},
            {"ISTJ","Responsible, detail-oriented, and reliable. Prefer clear rules and structure."},
            {"ISFJ","Caring, practical, and patient. Focus on helping others and preserving harmony."},
            {"ESTJ","Organized and pragmatic managers. Value efficiency and clear structures."},
            {"ESFJ","Friendly and cooperative. Prioritize community and supporting others."},
            {"ISTP","Practical problem-solvers. Calm and adaptable in crises; enjoy hands-on tasks."},
            {"ISFP","Sensitive, artistic, and spontaneous. Value personal freedom and aesthetics."},
            {"ESTP","Action-oriented and pragmatic. Enjoy risk-taking and solving immediate problems."},
            {"ESFP","Outgoing, playful, and sensory-focused. Enjoy living in the moment and entertaining others."}
        };

        private static readonly Dictionary<string, List<string>> Careers = new()
        {
            {"ENTJ", new(){"Project Manager","Product Manager","Senior Software Engineer","Operations Manager"}},
            {"INTJ", new(){"Systems Architect","Data Scientist","Research Engineer"}},
            {"ENFP", new(){"UX Designer","Marketing Specialist","Startup Founder"}},
            {"ISFJ", new(){"HR Specialist","Customer Support","Administrative Officer"}},
            {"ISTJ", new(){"Accountant","Quality Assurance","Backend Developer"}},
            
        };
        public async Task<PersonalityResult> AnalyzeAsync(PersonalityRequest request, bool useAiExplanation = true)
        {
            // validate length
            if (request?.Answers == null || request.Answers.Count != Questions.Count)
                throw new ArgumentException($"Answers must contain exactly {Questions.Count} items in correct order.");

            // scores per axis for left (L) and right (R)
            var axisLeft = new Dictionary<string, int> { { "EI", 0 }, { "SN", 0 }, { "TF", 0 }, { "JP", 0 } };
            var axisRight = new Dictionary<string, int> { { "EI", 0 }, { "SN", 0 }, { "TF", 0 }, { "JP", 0 } };

            // assume answers in 1..5 scale. Normalize if different.
            for (int i = 0; i < Questions.Count; i++)
            {
                var q = Questions[i];
                int ans = Math.Clamp(request.Answers[i], 1, 5); // ensure within 1..5
                // We will add the raw score to whichever side Polarity indicates.
                if (q.Polarity == 'L') axisLeft[q.Axis] += ans;
                else axisRight[q.Axis] += ans;
            }

            // Derive letter for each axis
            string letterEI = axisLeft["EI"] >= axisRight["EI"] ? "E" : "I";
            string letterSN = axisLeft["SN"] >= axisRight["SN"] ? "S" : "N";
            string letterTF = axisLeft["TF"] >= axisRight["TF"] ? "T" : "F";
            string letterJP = axisLeft["JP"] >= axisRight["JP"] ? "J" : "P";

            var mbti = $"{letterEI}{letterSN}{letterTF}{letterJP}";

            // CareerScore calculation (confidence)
            // Each axis maximum per side = 5*5 = 25. Difference range 0..25.
            int diffEI = Math.Abs(axisLeft["EI"] - axisRight["EI"]);
            int diffSN = Math.Abs(axisLeft["SN"] - axisRight["SN"]);
            int diffTF = Math.Abs(axisLeft["TF"] - axisRight["TF"]);
            int diffJP = Math.Abs(axisLeft["JP"] - axisRight["JP"]);

            int maxPerAxis = 5 * 5; // 25
            double normalized = (diffEI + diffSN + diffTF + diffJP) / (4.0 * maxPerAxis); // 0..1
            int careerScore = (int)Math.Round(normalized * 100);

            // Build result
            var result = new PersonalityResult
            {
                PersonalityType = mbti,
                CareerScore = careerScore,
                Description = Descriptions.ContainsKey(mbti) ? Descriptions[mbti] : "No description available.",
                AxisScores = new Dictionary<string, int>
                {
                    {"E", axisLeft["EI"]}, {"I", axisRight["EI"]},
                    {"S", axisLeft["SN"]}, {"N", axisRight["SN"]},
                    {"T", axisLeft["TF"]}, {"F", axisRight["TF"]},
                    {"J", axisLeft["JP"]}, {"P", axisRight["JP"]}
                },
                SuggestedCareers = Careers.ContainsKey(mbti) ? Careers[mbti] : new List<string>()
            };
           //AI = null;
            // Optional AI explanation: generate two paragraphs describing strengths & ideal careers
            if (useAiExplanation)
            {
                try
                {
                    var prompt = GenerateAiPrompt(result);
                    var aiResponse = await _AI.OpenAITurboModelAsync(prompt, "");

                   
                    if (!string.IsNullOrWhiteSpace(aiResponse))
                    {
                   
                        var cleaned = aiResponse.Trim();
                        if (cleaned.StartsWith("```"))
                        {
                            cleaned = cleaned.Trim('`').Trim();
                        }
                        result.Description += "\n\n" + cleaned;
                    }
                }
                catch
                {
                    // ignore AI errors - we already have a base description
                }
            }

            return result;
        }

        public async Task<bool> SaveAssessmentResultAsync(long? userId, Guid? tempSessionId, PersonalityRequest request, PersonalityResult result)
        {
            string answerJson = JsonConvert.SerializeObject(request.Answers);
            string query = @"Insert into Assesments
                           (UserId, TempSessionId, Answer, TotalScore, PersonalityType, CompletedAt, AssessmentType)
                           VALUES 
                          (@UserId, @TempSessionId, @Answer, @TotalScore, @PersonalityType, GETDATE(), @AssessmentType)";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.Parameters.AddWithValue("@TempSessionId", (object?)tempSessionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Answer", answerJson);
            cmd.Parameters.AddWithValue("@TotalScore", result.CareerScore);
            cmd.Parameters.AddWithValue("@PersonalityType", result.PersonalityType);
            cmd.Parameters.AddWithValue("@AssessmentType", "Personality");

            bool insert = await DBEngine.ExecuteNonQueryAsync(cmd, Databaseoperations.Insert, query);
            if (insert)
            {
                return true;
            }
            return false;


        }


        private string GenerateAiPrompt(PersonalityResult result)
        {
            // Keep prompt short and explicit
            return $@"You are an expert career counselor. 
User MBTI: {result.PersonalityType}
CareerScore: {result.CareerScore}
Provide TWO short paragraphs (2-4 sentences each):
1) Strengths and typical work style of people with this MBTI.
2) 3-5 ideal career types/roles and quick advice on how they should present themselves to employers.
Respond in plain text only, no JSON.";
        }


        

        // expose Questions for frontend to render
        public static List<QuestionItem> GetQuestions() => Questions.Select(q =>q).ToList();
    }
}
