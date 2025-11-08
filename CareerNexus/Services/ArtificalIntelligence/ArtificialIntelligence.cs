using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace CareerNexus.Services.ArtificalIntelligence
{
    public class ArtificialIntelligence:IArtificialIntelligence
    {
        private readonly HttpClient _client;
        private readonly ILogger<ArtificialIntelligence> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        public ArtificialIntelligence(HttpClient client, IConfiguration config, ILogger<ArtificialIntelligence> logger)
        {
            _client = client;
            _logger = logger;
            _apiKey = config["AI:ApiKey"];
            _apiUrl = config["AI:ApiUrl"];
        }
        public async Task<string> OpenAITurboModelAsync(string prompt, string resumeText)
        {
            try
            {
                //string apiKey = "2ZlLJj87ueRboSsZzLG0mMKX2jQbe62q";
                //string apiUrl = "https://api.mistral.ai/v1/chat/completions";

                var requestData = new
                {
                    model = "mistral-small-latest",
                    messages = new[] {
                new { role = "system", content = "You are an expert career advisor." },
                new { role = "user", content = $"{prompt}\n\nRESUME:\n{resumeText}" }
            }
                };

                var json = JsonConvert.SerializeObject(requestData);
                using var req = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                using var resp = await _client.SendAsync(req, HttpCompletionOption.ResponseContentRead);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AI API failed: {Code} {Body}", resp.StatusCode, body);
                    throw new Exception("AI API error");
                }

                // Return raw body. Caller will parse JSON to get content.
                return body;
            }
            catch(Exception ex){
                throw;

            }
        }
    }
    }
    
    
