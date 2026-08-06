using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/collectorbill")]
    public class ManageCollectorBillingController : ControllerBase
    {
        private readonly ICollectorBillingService _collectorBillingService; 
        public ManageCollectorBillingController(ICollectorBillingService collectorBillingService) { _collectorBillingService = collectorBillingService; }

        [HttpGet("get-bill")]
        public async Task<ActionResult<CollectorBillDto>?> GetWithdrawalHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            try
            {
                var result = await _collectorBillingService.GetBill(userId);

                if (result.IsFailure)
                    return BadRequest(result.Error);     

                if (result.Value is null)
                    return NotFound();                   

                return Ok(result.Value);                  
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception)
            {
                return Problem("Unable to retrieve current bill");
            }




        }

    }
}
