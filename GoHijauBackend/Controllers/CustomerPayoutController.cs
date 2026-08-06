using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payout")]
    public class CustomerPayoutController : ControllerBase
    {
        private readonly IPayoutService _payoutService;

        public CustomerPayoutController(IPayoutService payoutService)
        {
            _payoutService = payoutService;
        }

        [HttpPost("getStatus")]
        public async Task<IActionResult> GetStatus([FromForm] string reference)
        {
            try
            {
                PayoutStatus result = await _payoutService.GetPayoutStatus(reference);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("customer")]
        public async Task<IActionResult> CustomerPayout([FromBody] PayoutRequest payoutRequest)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _payoutService.CreatePayout(payoutRequest, userId);
                return Ok(new { success = result.Success, data = result });
            }
            catch (Exception ex)
            {
                //Later Put logging to keep record of fails also
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
