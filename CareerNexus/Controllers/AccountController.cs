using CareerNexus.Models;
using CareerNexus.Models.ChangePassword;
using CareerNexus.Models.Common;
using CareerNexus.Models.RequestModel;
using CareerNexus.Models.UserModel;
using CareerNexus.Services;
using CareerNexus.Services.Authenticate;
using CareerNexus.Services.Feedback;
using CareerNexus.Services.OtpService;
using CareerNexus.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace CareerNexus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticate _authenticationservice;
        private readonly IOTP _otp;
        private readonly IConfiguration _config;
        private readonly IUserService _userservice;
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<AccountController> _logger ;

        public AccountController(IAuthenticate authenticationService,IFeedbackService feedbackService, IConfiguration config, IOTP otpservice,IUserService userService,ILogger<AccountController> logger
            )
        {
           _userservice = userService;
            _feedbackService = feedbackService;
            _logger = logger;
            _otp = otpservice;
            _authenticationservice = authenticationService;
            _config = config;
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationRequestModel), 200)]
        [HttpPost("Register")]
        public async Task <IActionResult> Register(UserRequestModel user)
        {
            //var userId = Convert.ToInt64(User.FindFirst(ClaimTypes.PrimarySid)?.Value);
            
            long result = await _authenticationservice.Register(user);
            string msg = string.Empty;
            bool isSuccess = false;
            if (result > 0)
            {
                msg = "User Create Successfully";
                isSuccess = true;
            }
            else
            {
                msg = "Email already Exist";
                isSuccess = false;
            }
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    StatusCode=(int)HttpStatusCode.OK,
                    Data = result,
                    Message = msg,
                    IsSuccess = isSuccess
                });
           
        }
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationRequestModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        [HttpPost("login")]
        public async Task<IActionResult> Authenticate(AuthenticationRequestModel request)
        {
            var (message, result) = await _authenticationservice.Authenticate(request);
            if (result != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = result,
                    Message=message,
                    IsSuccess=true
                });
            }

            return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
            {
                
                Message = message,
                IsSuccess = false
            });
        }
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationRequestModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                bool result = await _userservice.ForgotPassword(request.Email);

                if (!result)
                {
                    return NotFound(new { message = "User not found or unable to reset password" });
                }

                return Ok(new { message = "Password reset successfully. Please check your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPassword for email {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        [HttpPost("ChangePassword")]
        [ProducesResponseType(typeof(SuccessResponseModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> ChangePassword(ChangePassword changePasswordModel) 
        {
            var userId = Convert.ToInt64(User.FindFirst(ClaimTypes.PrimarySid)?.Value);
            changePasswordModel.UserId = userId; 

            try
            { 
                _logger.LogInformation($"Going to change password. UserId: {changePasswordModel.UserId}"); 
                string message = string.Empty;
                var IsChanged = await _userservice.ChangePassword(changePasswordModel);
                if (IsChanged.Item1) 
                { return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                { Message = "Password changed successfully.",
                    Data = IsChanged, StatusCode = (int)HttpStatusCode.OK,
                    IsSuccess = true
                });
                } else { 
                    return StatusCode((int)HttpStatusCode.BadRequest, new SuccessResponseModel { 
                        Message = IsChanged.Item2, 
                        Data = IsChanged.Item1,
                        StatusCode = (int)HttpStatusCode.BadRequest, 
                        IsSuccess = false 
                    }); 
                } 
            }
            catch (Exception ex) { 
                var message = $"Error occured while changing password, Error = {ex}";
                _logger.LogError(message);
                return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel {
                    IsSuccess = false, 
                    StatusCode = (int)HttpStatusCode.BadRequest, 
                    Message = "Some error occurred!", 
                    Error = new List<object> { new { message = message } },
                    IsException = true 
                });
            } 
        }
        [Authorize]
        [HttpPost("SubmitFeedback")]
        [ProducesResponseType(typeof(SuccessResponseModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackSubmitRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { message = "Message is required." });
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.PrimarySid)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                long feedbackId = await _feedbackService.SubmitFeedbackAsync(userId, request.Message.Trim(), request.FeedbackType ?? "suggestion");
                if (feedbackId <= 0)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "Failed to save feedback." });
                }

                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = feedbackId,
                    Message = "Feedback submitted successfully.",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubmitFeedback failed");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = "An error occurred while submitting feedback.",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("GetFeedbackByUserId")]
        [ProducesResponseType(typeof(SuccessResponseModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]
        public async Task<IActionResult> GetFeedbackByUser()
        {
            try
            {
                var userIdClaim = Convert.ToInt64(User.FindFirst(ClaimTypes.PrimarySid)?.Value);
                var result = await _feedbackService.GetFeedbackByUserIdAsync(userIdClaim);


                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = result,
                    Message = "Feedback fetched successfully.",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });
            }
            catch(Exception ex)
            { 
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponseModel
                {
                    Message = $"An error occurred while fetching feedback.",
                    IsSuccess = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
        
        [HttpDelete("DeleteFeedbackById")]
        [ProducesResponseType(typeof(SuccessResponseModel), 200)]
        [ProducesResponseType(typeof(ErrorResponseModel), 400)]

        public async Task<IActionResult> DeleteFeedback(long id)
        {
            long? userId = null;

            var claim = User.FindFirst(ClaimTypes.PrimarySid)?.Value;
            if (!string.IsNullOrEmpty(claim))
                userId = Convert.ToInt64(claim);

            if (userId == null)
                return Unauthorized();

            //bool isAdmin = User.IsInRole("Admin");

            bool deleted = await _feedbackService.DeleteFeedbackAsync(id, userId.Value);

            if (!deleted)
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = deleted,
                    Message = "Invalid Id OR ID not found",
                    IsSuccess = true,
                    StatusCode = (int)HttpStatusCode.OK
                });

            return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
            {
                Data = deleted,
                Message = "Feedback delete successfully.",
                IsSuccess = true,
                StatusCode = (int)HttpStatusCode.OK
            });
        }

    }

}
