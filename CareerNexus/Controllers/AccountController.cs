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
                    IsSuccess = true
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

    }

}
