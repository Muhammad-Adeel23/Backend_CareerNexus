using CareerNexus.Services.Admin;
using CareerNexus.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using CareerNexus.Services.Feedback;

namespace CareerNexus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService,IFeedbackService feedbackService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _feedbackService = feedbackService;
            _logger = logger;
        }

        [HttpGet("Overview")]
        public async Task<IActionResult> GetOverview()
        {
            try
            {
                var stats = await _adminService.GetOverviewStatsAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel

                {
                    Data = stats,
                    Message = "Overview stats retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overview");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving overview stats",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _adminService.GetUsersAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = users,
                    Message = "Users retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving users",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var user = await _adminService.GetUserAsync(id);
                if (user == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, new ErrorResponseModel
                    {
                        Message = "User not found",
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.NotFound
                    });
                }

                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = user,
                    Message = "User retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving user",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPut("User/{id}/Status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                var result = await _adminService.UpdateUserStatusAsync(id, request.IsActive);
                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "User status updated successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to update user status",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status {UserId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error updating user status",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpDelete("User/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(id);
                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "User deleted successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to delete user",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error deleting user",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Assessments")]
        public async Task<IActionResult> GetAssessments()
        {
            try
            {
                var assessments = await _adminService.GetAssessmentsAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = assessments,
                    Message = "Assessments retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessments");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving assessments",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Assessment/{id}")]
        public async Task<IActionResult> GetAssessment(int id)
        {
            try
            {
                var assessment = await _adminService.GetAssessmentAsync(id);
                if (assessment == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound, new ErrorResponseModel
                    {
                        Message = "Assessment not found",
                        IsSuccess = false,
                        StatusCode = (int)HttpStatusCode.NotFound
                    });
                }

                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = assessment,
                    Message = "Assessment retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment {AssessmentId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving assessment",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Resumes")]
        public async Task<IActionResult> GetResumes()
        {
            try
            {
                var resumes = await _adminService.GetResumesAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = resumes,
                    Message = "Resumes retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resumes");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving resumes",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Careers")]
        public async Task<IActionResult> GetCareers()
        {
            try
            {
                var careers = await _adminService.GetCareersAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = careers,
                    Message = "Careers retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting careers");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving careers",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPost("Career")]
        public async Task<IActionResult> CreateCareer([FromBody] CareerModel career)
        {
            try
            {
                var result = await _adminService.CreateCareerAsync(career);
                if (result != null)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "Career created successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to create career",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating career");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error creating career",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPut("Career/{id}")]
        public async Task<IActionResult> UpdateCareer(int id, [FromBody] CareerModel career)
        {
            try
            {
                var result = await _adminService.UpdateCareerAsync(id, career);
                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "Career updated successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to update career",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating career {CareerId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error updating career",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpDelete("Career/{id}")]
        public async Task<IActionResult> DeleteCareer(int id)
        {
            try
            {
                var result = await _adminService.DeleteCareerAsync(id);
                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "Career deleted successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to delete career",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting career {CareerId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error deleting career",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Skills")]
        public async Task<IActionResult> GetSkills()
        {
            try
            {
                var skills = await _adminService.GetSkillsAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = skills,
                    Message = "Skills retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving skills",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPost("Skill")]
        public async Task<IActionResult> CreateSkill([FromBody] SkillModel skill)
        {
            try
            {
                var result = await _adminService.CreateSkillAsync(skill);
                if (result != null)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "Skill created successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to create skill",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skill");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error creating skill",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPut("Skill/{id}")]
        public async Task<IActionResult> UpdateSkill(int id, [FromBody] SkillModel skill)
        {
            try
            {
                var result = await _adminService.UpdateSkillAsync(id, skill);
                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "Skill updated successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to update skill",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill {SkillId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error updating skill",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpDelete("Skill/{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            try
            {
                var result = await _adminService.DeleteSkillAsync(id);
                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "Skill deleted successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to delete skill",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skill {SkillId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error deleting skill",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Settings")]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var settings = await _adminService.GetSettingsAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = settings,
                    Message = "Settings retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settings");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving settings",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPut("Settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> settings)
        {
            try
            {
                var result = await _adminService.UpdateSettingsAsync(settings);
                if (result)
                {
                    return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                    {
                        Data = result,
                        Message = "Settings updated successfully",
                        IsSuccess = true,
                        StatusCode = (int)HttpStatusCode.OK
                    });
                }

                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
                {
                    Message = "Failed to update settings",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error updating settings",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("Stats/TopCareers")]
        public async Task<IActionResult> GetTopCareers()
        {
            try
            {
                var topCareers = await _adminService.GetTopCareersAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = topCareers,
                    Message = "Top careers retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top careers");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving top careers",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
        [HttpGet("Feedback")]
        public async Task<IActionResult> GetFeedback()
        {
            try
            {
                var list = await _feedbackService.GetAllFeedbackAsync();
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = list,
                    Message = "Feedback retrieved successfully",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "Error retrieving feedback",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
