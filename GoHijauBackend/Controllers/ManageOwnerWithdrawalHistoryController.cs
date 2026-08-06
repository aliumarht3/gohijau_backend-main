using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/owner/withdrawal")]
    public class ManageOwnerWithdrawalHistoryController : ControllerBase
    {
        private readonly IOwnerWithdrawawalHistoryService _ownerWithdrawawalHistoryService;

        public ManageOwnerWithdrawalHistoryController(IOwnerWithdrawawalHistoryService ownerWithdrawawalHistoryService) 
        { 
            _ownerWithdrawawalHistoryService = ownerWithdrawawalHistoryService;
        }

        [HttpGet("get-withdrawal-history")]
        public async Task<ActionResult<List<WithdrawalHistory>>> GetWithdrawalHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            try
            {
                var result = await _ownerWithdrawawalHistoryService.GetWithdrawalHistory(userId);
                return Ok(result?.ToList() ?? new List<MachineOwnerWithdrawalHistory>());
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return Problem("Unable to retrieve withdrawal history right now.");
            }
        }
    }
}
