using CareerNexus.Services.Personality;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static CareerNexus.Models.PersonalityModels;

namespace CareerNexus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalityController : ControllerBase
    {
        private readonly IPersonalityService _service;

        public PersonalityController(IPersonalityService service)
        {
            _service = service;
        }

        [HttpGet("questions")]
        public IActionResult GetQuestions()
        {
            var qs = PersonalityService.GetQuestions();
            return Ok(new { message = "Questions loaded", data = qs });
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] PersonalityRequest req, [FromQuery] bool useAi = true)
        {
            var userId = Convert.ToInt64(User.FindFirst(ClaimTypes.PrimarySid)?.Value);
            if (req == null || req.Answers == null)
                return BadRequest(new { message = "Invalid request" });

            try
            {
                var result = await _service.AnalyzeAsync(req, useAi);
                bool saved = await _service.SaveAssessmentResultAsync(
                             userId: userId > 0 ? userId : (long?)null, 
                             tempSessionId: Guid.NewGuid(),           
                             request: req,
                             result: result
       );

                return Ok(new { message = "Personality analyzed", data = result, isSuccess = true });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // log ex
                return StatusCode(500, new { message = "Server error" });
            }
        }

    }
}
