using CareerNexus.Models.Resume;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace CareerNexus.Services.ArtificalIntelligence
{
    public class ArtificialIntelligence:IArtificialIntelligence
    {
        private readonly string _rapidApiKey = "cc796df2e5msh955f89e95801e61p18327bjsnc45a1210f67c"; // 🔑 Your RapidAPI key


        private readonly HttpClient _client;
        private readonly ILogger<ArtificialIntelligence> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _apiId;
        private readonly string _JoobleApiKey;
        public ArtificialIntelligence(HttpClient client, IConfiguration config, ILogger<ArtificialIntelligence> logger)
        {
            _client = client;
            _logger = logger;
            _apiKey = config["AI:ApiKey"];
            _apiUrl = config["AI:ApiUrl"];
            _apiId = config["RapidApi:ApiId"];
            _JoobleApiKey = config["RapidApi:ApiKey"];
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

        public async Task<Dictionary<string, List<JobVacancy>>> GetJobVacanciesAsync(List<string> careers, string city)
        {
            var vacancies = new Dictionary<string, List<JobVacancy>>();
            using var client = new HttpClient();

            // RapidAPI required headers
            client.DefaultRequestHeaders.Add("x-rapidapi-host", "jsearch.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("x-rapidapi-key", _rapidApiKey);

            foreach (var career in careers)
            {
                try
                {
                    string encodedCareer = Uri.EscapeDataString(career);
                    string encodedCity = Uri.EscapeDataString(city);

                    string url = $"https://jsearch.p.rapidapi.com/search?query={encodedCareer}%20in%20{encodedCity}&num_pages=1";

                    var response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"⚠️ JSearch returned {response.StatusCode} for {career}");
                        vacancies[career] = new List<JobVacancy>();
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(json);

                    var jobList = new List<JobVacancy>();
                    DateTime cutoffDate = DateTime.UtcNow.AddDays(-60); // last 90 days only

                    if (result?.data != null)
                    {
                        foreach (var job in result.data)
                        {
                            DateTime? postedAt = null;
                            try
                            {
                                if (job.job_posted_at_datetime_utc != null)
                                    postedAt = DateTime.Parse(job.job_posted_at_datetime_utc.ToString()).ToUniversalTime();
                            }
                            catch { }

                            // ❌ Skip if date missing or too old
                            if (postedAt == null || postedAt < cutoffDate)
                                continue;

                            jobList.Add(new JobVacancy
                            {
                                Title = job.job_title ?? "N/A",
                                Company = job.employer_name ?? "Unknown",
                                Location = job.job_city ?? "Unknown",
                                Url = job.job_apply_link ?? "",
                                PostedAt = postedAt
                            });
                        }
                    }

                    vacancies[career] = jobList;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error fetching {career}: {ex.Message}");
                    vacancies[career] = new List<JobVacancy>();
                }
            }

            return vacancies;
        }

        //public async Task<Dictionary<string, List<JobVacancy>>> GetJobVacanciesAsync(List<string> careers, string city)
        //{
        //    var vacancies = new Dictionary<string, List<JobVacancy>>();

        //    foreach (var career in careers)
        //    {
        //        try
        //        {
        //            // Encode parameters to make URL safe
        //            string encodedCareer = HttpUtility.UrlEncode(career);
        //            string encodedCity = HttpUtility.UrlEncode(city);

        //            string url = $"https://www.rozee.pk/search/jobs?q={encodedCareer}&l={encodedCity}";
        //            string html = await _client.GetStringAsync(url);

        //            var doc = new HtmlDocument();
        //            doc.LoadHtml(html);

        //            var jobList = new List<JobVacancy>();

        //            // Select job cards
        //            var jobNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'jobListing')]");

        //            if (jobNodes != null)
        //            {
        //                foreach (var jobNode in jobNodes)
        //                {
        //                    try
        //                    {
        //                        var titleNode = jobNode.SelectSingleNode(".//a[contains(@class,'job-title')]");
        //                        var companyNode = jobNode.SelectSingleNode(".//span[contains(@class,'job-company')]");
        //                        var locationNode = jobNode.SelectSingleNode(".//span[contains(@class,'job-location')]");

        //                        var job = new JobVacancy
        //                        {
        //                            Title = titleNode?.InnerText.Trim() ?? "N/A",
        //                            Company = companyNode?.InnerText.Trim() ?? "Unknown",
        //                            Location = locationNode?.InnerText.Trim() ?? city,
        //                            Url = "https://www.rozee.pk" + titleNode?.GetAttributeValue("href", "#")
        //                        };

        //                        jobList.Add(job);
        //                    }
        //                    catch
        //                    {
        //                        // Skip any incomplete job record
        //                    }
        //                }
        //            }

        //            vacancies[career] = jobList;
        //        }
        //        catch
        //        {
        //            vacancies[career] = new List<JobVacancy>();
        //        }
        //    }

        //    return vacancies;
        //}



    }
    }
    
    
