using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class ManageCollectorTransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public ManageCollectorTransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("collector/record-uco-weight")]
        public async Task<IActionResult> RecordCollectorUCOWeight([FromForm] string ucoWeight)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                double collectorRecordedOil = double.Parse(ucoWeight);

                await _transactionService.InsertCollectorRecordedUCOTransactionAsync(userId, collectorRecordedOil);

                return Ok(new { success = true, message = "UCO weight recorded: " + ucoWeight });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("collector/history")]
        public async Task<IActionResult> GetCollectorHistory()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated." });
                }

                var history = await _transactionService.GetCollectorHistoryAsync(userId);

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching collector history.", details = ex.Message });
            }
        }
    }
}
