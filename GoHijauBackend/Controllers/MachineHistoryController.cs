using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/machine")]
    public class MachineHistoryController : ControllerBase
    {
        private readonly IMachineHistoryService _machineHistoryService;

        public MachineHistoryController(IMachineHistoryService machineHistoryService)
        {
            _machineHistoryService = machineHistoryService;
        }

        [HttpGet("{machineId}/deposition-history")]
        public async Task<IActionResult> GetDepositionHistory(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                return BadRequest(new { message = "Machine ID is required." });

            try
            {
                var history = await _machineHistoryService.GetDepositionHistoryByMachineIdAsync(machineId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch deposition history.", error = ex.Message });
            }
        }

        [HttpGet("{machineId}/collection-history")]
        public async Task<IActionResult> GetCollectionHistory(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                return BadRequest(new { message = "Machine ID is required." });

            try
            {
                var history = await _machineHistoryService.GetCollectionHistoryByMachineIdAsync(machineId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch collection history.", error = ex.Message });
            }
        }
    }
}
