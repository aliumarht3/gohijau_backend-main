using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Services;
using GoHijauBackend.Domain.Entities.External;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/machineowner")]
    public class ManageMachineOwnerPayoutController : ControllerBase
    {
        private readonly IMachineOwnerPayoutService _machineOwnerPayoutService;
        public ManageMachineOwnerPayoutController(IMachineOwnerPayoutService machineOwnerPayoutService)
        {
            _machineOwnerPayoutService = machineOwnerPayoutService;
        }
        [HttpPost("getStatus")]
        public async Task<IActionResult> GetStatus([FromForm] string reference)
        {
            try
            {
                PayoutStatus result = await _machineOwnerPayoutService.GetPayoutStatus(reference);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
        [HttpPost("payout")]
        public async Task<IActionResult> MachineOwnerPayout([FromBody] PayoutRequest payoutRequest)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _machineOwnerPayoutService.CreatePayout(payoutRequest, userId);
                return Ok(new { success = result.Success, data = result });
            }
            catch (Exception ex)
            {
                //Later Put logging to keep record of fails also
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
        [HttpPost("populate-total-transaction")]
        public async Task<IActionResult> PopulateTotalMachineOwnerTransaction(string organizationId)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationId) || string.IsNullOrWhiteSpace(organizationId)) {
                    return StatusCode(400, new { success = false, error = "Organization not found" });
                }
                await _machineOwnerPayoutService.PopulateTotalTransactionMachineOwner(organizationId);
                return Ok(new { success ="Total Transactions for Machine Owner Populated"});
            }
            catch (Exception ex)
            {
                //Later Put logging to keep record of fails also
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

    }
}
