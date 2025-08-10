using CareerNexus.Models;
using CareerNexus.Models.Common;
using CareerNexus.Models.RequestModel;
using CareerNexus.Models.UserModel;
using CareerNexus.Services;
using CareerNexus.Services.Authenticate;
using CareerNexus.Services.OtpService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public AccountController(IAuthenticate authenticationService, IConfiguration config, IOTP otpservice)
        {
            _otp = otpservice;
            _authenticationservice = authenticationService;
            _config = config;
        }

        [HttpPost]
        public IActionResult signup(UserModel user)
        {
            var result = _authenticationservice.Signup(user);
            if (result != null)
            {
                return Ok(new
                {
                    message = "Signup successful",
                    user = new
                    {
                        username = result.Username,
                        email = result.Email,
                        fullname = result.Fullname
                    }
                });
            }
            return BadRequest(new { message = "Signup failed" });
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

    }

}
