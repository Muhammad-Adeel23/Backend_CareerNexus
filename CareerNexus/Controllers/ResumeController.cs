using CareerNexus.Common;
using CareerNexus.Models.Common;
using CareerNexus.Models.Resume;
using CareerNexus.Services.ArtificalIntelligence;
using CareerNexus.Services.CareerRecommendation;
using CareerNexus.Services.ResumeAnalyzer;
using CareerNexus.Services.ResumeParser;
using CareerNexus.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HPSF;
using System.Data;
using System.Net;
using System.Security.Claims;

namespace CareerNexus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IArtificialIntelligence _aiservce;
        private readonly IResumeParser _parser;
        private readonly IResumeAnalyzer _analyzer;
        private readonly ICareerRecommendationService _careerService;

        public ResumeController(
            IStorageService storageService,
            IArtificialIntelligence aiservce,
            IResumeParser parser,
            IResumeAnalyzer analyzer,
            ICareerRecommendationService careerService
        /* plus logger, config, etc. */
        )
        {
            _storageService = storageService;
            _parser = parser;
            _aiservce = aiservce;
            _analyzer = analyzer;
            _careerService = careerService;
        }
        [AllowAnonymous]
        [HttpPost("UploadResume")]
        [Authorize]
        public async Task<IActionResult> UploadResume([FromForm] ResumeUploadRequest request)
        {
            try
            {
                long? userId = null;

                var userIdClaim = User.FindFirst(ClaimTypes.PrimarySid)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var parsed))
                    userId = parsed;

                if (request.ResumeFile == null || request.ResumeFile.Length == 0)
                    return BadRequest("No resume uploaded.");

                var allowed = new[] { ".pdf", ".docx", ".doc" };
                var ext = Path.GetExtension(request.ResumeFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext)) return BadRequest("Unsupported file type.");
                if (request.ResumeFile.Length > 5 * 1024 * 1024) // 5MB
                    return BadRequest("File too large.");

                // store file (returns stored path or url)
                var storedFilePath = await _storageService.SaveFileAsync(request.ResumeFile, userId);

                // extract text
                var text = await _parser.ExtractTextFromFileAsync(request.ResumeFile);
                if (string.IsNullOrWhiteSpace(text))
                    return BadRequest("The uploaded resume appears to be empty or unreadable.");

                
                // analyze via AI
                var analysis = await _analyzer.AnalyzeResumeAsync(text);
                if (analysis.MissingSkills.Any() == true)
                
                    analysis.Tutorials = await _analyzer.GetTutorialLinksAsync(analysis.MissingSkills);
                string city = "Karachi";
                if (analysis.CareerRecommendation?.Any() == true)
                    
                    analysis.JobVacancies = await _aiservce.GetJobVacanciesAsync(analysis.CareerRecommendation, city); // city can be parameterized



                // save to DB
                var saved = await SaveResumeToDbAsync(userId, storedFilePath, analysis);
                if (!saved) return StatusCode(500, "Failed to save resume.");

                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = analysis,
                    Message = "Resume analyze Successfully",
                    IsSuccess = true
                });
            }
            catch (Exception ex)
            {
                // log ex
                return StatusCode(500, "An error occurred while processing resume.");
            }
        }

        private Task<bool> SaveResumeToDbAsync(long? userId, string fileUrl, ResumeAnalysisResult analysis)
        {
            const string query = @"
INSERT INTO Resumes (UserId, FileURL, ParsedSkills, Analysis, UploadedAt)
VALUES (@UserId, @FileURL, @ParsedSkills, @Analysis, GETDATE());";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@UserId", SqlDbType.BigInt).Value = userId ?? (object)DBNull.Value;
            cmd.Parameters.Add("@FileURL", SqlDbType.NVarChar, 500).Value = fileUrl ?? string.Empty;
            cmd.Parameters.Add("@ParsedSkills", SqlDbType.NVarChar, -1).Value = string.Join(",", analysis?.MatchedSkills ?? new List<string>());
            cmd.Parameters.Add("@Analysis", SqlDbType.NVarChar, -1).Value = JsonConvert.SerializeObject(analysis ?? new ResumeAnalysisResult());

            // DBEngine.ExecuteNonQuery returns bool in your code. Wrap into Task.FromResult
            bool success = DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Insert, query);
            return Task.FromResult(success);
        }


    }
}
