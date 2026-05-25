using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/UCOTracking")]
    public class ManageMachineUCOTrackingController : ControllerBase
    {
        private readonly IMachineUCOTrackingService _service;
        public ManageMachineUCOTrackingController(IMachineUCOTrackingService service)
        {
            _service = service;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewMachineTracking([FromBody] MachineUCOTrackingDTO uCOTrackingDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.CreateNewUCOMachineTracking(uCOTrackingDTO.MachineId,uCOTrackingDTO.BufferVolume);

                if (result == null)
                    return BadRequest(new { message = "Error creating uco tracking" });

                return Ok(new { message = "New UCOTracking added Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateMachineTracking([FromBody] MachineUCOTrackingDTO uCOTrackingDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.UpdateMachineUCOTracking(uCOTrackingDTO);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Updated UCOTracking successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpGet("get-collector")]
        public async Task<IActionResult> GetMachineTrackerByCollectorId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "User not authenticated." });

            try
            {
                var list = await _service.GetMachineTrackerByCollectorId(userId);

                // Return an array (possibly empty) so the app can render predictably.
                return Ok(list ?? new List<MachineUCOTracking>());
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { message = "Request was cancelled." }); // or 408 per your policy
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("get-owner")]
        public async Task<IActionResult> GetMachineTrackerByOwnerId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "User not authenticated." });

            try
            {
                var list = await _service.GetMachineTrackerByOwnerId(userId);

                // Return an array (possibly empty) so the app can render predictably.
                return Ok(list ?? new List<MachineUCOTracking>());
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { message = "Request was cancelled." }); // or 408 per your policy
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetMachineAllTracker()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "User not authenticated." });

            try
            {
                var list = await _service.GetAllMachineTracker();

                // Return an array (possibly empty) so the app can render predictably.
                return Ok(list ?? new List<MachineUCOTracking>());
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { message = "Request was cancelled." }); // or 408 per your policy
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
