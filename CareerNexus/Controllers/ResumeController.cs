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
       
        public async Task<IActionResult> UploadResume([FromForm] ResumeUploadRequest request)
        {
            try
            {
                long? userId = null;

                var userIdClaim = User.FindFirst(ClaimTypes.PrimarySid)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var parsed))
                    userId = parsed;

                if (userId == null && request.TempSessionId == null)
                    
                    return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                    {
                        Message = "Temp session is required for guest users.",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    });

                if (request.ResumeFile == null || request.ResumeFile.Length == 0)
                    return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                    {
                        Message = "No Resume Uploaded.",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    });

                var allowed = new[] { ".pdf", ".docx", ".doc" };
                var ext = Path.GetExtension(request.ResumeFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                    return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                    {
                        Message = "UnSupported file type.",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    });
                if (request.ResumeFile.Length > 5 * 1024 * 1024) // 5MB
                    return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                    {
                        Message = "File too large.",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    });

                // store file (returns stored path or url)
                var storedFilePath = await _storageService.SaveFileAsync(request.ResumeFile, userId);

                // extract text
                var text = await _parser.ExtractTextFromFileAsync(request.ResumeFile);
                if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 50)
                {
                    //return BadRequest("Resume is blank or does not contain readable text. Please upload a valid resume.");
                    return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                    {
                        Message = "Resume is blank or does not contain readable text. Please upload a valid resume.",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    });
                }


                // analyze via AI
                var analysis = await _analyzer.AnalyzeResumeAsync(text);
                if (analysis.MissingSkills.Any() == true)
                
                    analysis.Tutorials = await _analyzer.GetTutorialLinksAsync(analysis.MissingSkills);
                string city = "Karachi";
                if (analysis.CareerRecommendation?.Any() == true)
                    
                    analysis.JobVacancies = await _aiservce.GetJobVacanciesAsync(analysis.CareerRecommendation, city); // city can be parameterized



                // save to DB
                var saved = await SaveResumeToDbAsync(userId, request.TempSessionId, storedFilePath, analysis);
                if (!saved)
                    return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                    {
                        Message = "Failed to save resume.",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    });

                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = analysis,
                    Message = "Resume analyze Successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK

                });
            }
            catch (Exception ex)
            {
                // log ex
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "An error occured while processing resume.",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [Authorize]
        [HttpPost("MigrateGuestData")]
        public IActionResult MigrateGuestData(MigrateGuestDataRequest request)
        {
            if (request.TempSessionId == Guid.Empty)
                return BadRequest("TempSessionId is required.");

            var userIdClaim = User.FindFirst(ClaimTypes.PrimarySid)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("UserId not found in token.");

            long userId = long.Parse(userIdClaim);
            var result  = _parser.MigrateUserData(request,userId);
            if (result)
            {

                return Ok(new SuccessResponseModel
                {
                    IsSuccess = true,
                    Message = "Guest data migrated successfully"
                });
            }
            return StatusCode(500, "Failed to migrate guest data.");

        }


        private Task<bool> SaveResumeToDbAsync(long? userId, Guid? tempSessionId, string fileUrl, ResumeAnalysisResult analysis)
        {
            const string query = @"
INSERT INTO Resumes (UserId, TempSessionId, FileURL, ParsedSkills, Analysis, UploadedAt)
VALUES (@UserId,@TempSessionId, @FileURL, @ParsedSkills, @Analysis, GETDATE());";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add("@UserId", SqlDbType.BigInt).Value = userId ?? (object)DBNull.Value;
            cmd.Parameters.Add("@TempSessionId", SqlDbType.UniqueIdentifier) .Value = tempSessionId ?? (object)DBNull.Value;
            cmd.Parameters.Add("@FileURL", SqlDbType.NVarChar, 500).Value = fileUrl ?? string.Empty;
            cmd.Parameters.Add("@ParsedSkills", SqlDbType.NVarChar, -1).Value = string.Join(",", analysis?.MatchedSkills ?? new List<string>());
            cmd.Parameters.Add("@Analysis", SqlDbType.NVarChar, -1).Value = JsonConvert.SerializeObject(analysis ?? new ResumeAnalysisResult());

            // DBEngine.ExecuteNonQuery returns bool in your code. Wrap into Task.FromResult
            bool success = DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Insert, query);
            return Task.FromResult(success);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestResume()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.PrimarySid)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "User not found!" });

            var userId = Convert.ToInt64(userIdClaim);
            var resume = await _analyzer.GetLatestResume(userId);

            if (resume == null)
                return Ok(new { message = "No resume uploaded yet!" });

            // Parse the Analysis JSON to send structured data to frontend
            var analysisObj = _analyzer.ParseAnalysis(resume.Analysis);

            return Ok(new
            {
                fileURL = resume.FileURL,
                parsedSkills = resume.ParsedSkills,
                uploadedAt = resume.UploadedAt,
                analysis = analysisObj
            });
        }
        //[HttpGet("download-resume/{fileName}")]
        //public IActionResult DownloadResume(string fileName)
        //{
        //    var fullPath = Path.Combine(_basePath, fileName);

        //    if (!System.IO.File.Exists(fullPath))
        //        return NotFound("File not found");

        //    var bytes = System.IO.File.ReadAllBytes(fullPath);

        //    return File(bytes, "application/pdf", Path.GetFileName(fullPath));
        //}
    }
}
