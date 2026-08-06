using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    //[ApiController]
    //[Route("api/email")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _service;

        public EmailController(IEmailService service) {
            _service = service;
        }

        //[HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] EmailDto emailDto) 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.SendEmail(emailDto);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Email Sent Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
