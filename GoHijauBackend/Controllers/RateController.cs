using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/rate")]
    public class RateController : ControllerBase
    {
        private readonly IRateService _rateService;

        public RateController (IRateService rateService)
        {
            _rateService = rateService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> AddRate([FromBody] RateDTO rate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                rate.UserId = userId;
                var result = await _rateService.AddRate(rate);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Rate added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRate()
        {
            try
            {
                var rate = await _rateService.GetLatestRate();

                return Ok(rate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
