using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GoHijauBackend.Api.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api")]
    public class ManageCustomerWithdrawalHistory : ControllerBase
    {
        private readonly IWithdrawalHistoryService _withdrawalHistoryService;

        public ManageCustomerWithdrawalHistory(IWithdrawalHistoryService withdrawalHistoryService)
        {
            _withdrawalHistoryService = withdrawalHistoryService;
        }
        [HttpPost("customer/create-withdrawal-history")]
        public async Task<IActionResult> AddWithdrawalHistory(string amount, string userId)
        {
            try {
               var result =   await _withdrawalHistoryService.AddWithdrawalHistory(amount, userId);
                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "A history has been added", id = result.Id });
            } catch (Exception e) {

                return StatusCode(500, new { message = "An unexpected error occurred.", details = e.Message });
            }
            
        }
        [HttpGet("customer/get-withdrawal-history")]
        public async Task<ActionResult<List<WithdrawalHistory>>> GetWithdrawalHistory() {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            try
            {
                var result = await _withdrawalHistoryService.GetWithdrawalHistory(userId);
                // Normalize to empty list if service returns null
                return Ok(result?.ToList() ?? new List<WithdrawalHistory>());
            }
            catch (OperationCanceledException)
            {
                // Request was aborted by the client
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return Problem("Unable to retrieve withdrawal history right now.");
            }
        }
        [HttpPut("customer/update-withdrawal-status")]
        public async Task<IActionResult> UpdateWithdrawalHistoryStatus(string withdrawalId, string status)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(userId))
            //    return Unauthorized();
            try
            {
                var result = await _withdrawalHistoryService.UpdateWithdrawalStatus(withdrawalId, status);
                // Normalize to empty list if service returns null
                return Ok(new { message = "Status has been updated" });
            }
            catch (OperationCanceledException)
            {
                // Request was aborted by the client
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return Problem("Unable to retrieve withdrawal history right now.");
            }
        }
    }
}
