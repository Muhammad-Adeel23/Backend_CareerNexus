using CareerNexus.Models;
using CareerNexus.Models.Common;
using CareerNexus.Models.RequestModel;
using CareerNexus.Models.UserModel;
using CareerNexus.Services;
using CareerNexus.Services.Authenticate;
using CareerNexus.Services.OtpService;
using CareerNexus.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        private readonly ILogger<AccountController> _logger ;

        public AccountController(IAuthenticate authenticationService, IConfiguration config, IOTP otpservice,IUserService userService,ILogger<AccountController> logger
            )
        {
           _userservice = userService;
            _logger = logger;
            _otp = otpservice;
            _authenticationservice = authenticationService;
            _config = config;
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationRequestModel), 200)]
        [HttpPost("Register")]
        public async Task <IActionResult> Register(UserModel user)
        {
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
            var result = await _authenticationservice.Authenticate(request);
            if (result != null)
            {
                return StatusCode((int)HttpStatusCode.OK, new SuccessResponseModel
                {
                    Data = result,
                    Message="Login Successfully",
                    IsSuccess=true
                });
            }

            return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponseModel
            {
                
                Message = "Login failed",
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
    }

}
