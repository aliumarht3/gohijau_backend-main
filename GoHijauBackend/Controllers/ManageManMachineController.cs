using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/manufacturer")]
    public class ManageManMachineController : ControllerBase
    {
        private readonly IManMachineService _service;
        public ManageManMachineController(IManMachineService service) 
        {
            _service = service; 
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var machines = await _service.GetAllMachines();

                return Ok(machines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ManMachineDTO manMachineDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.ManufactureMachine(userId, manMachineDTO);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Machine Manufactured Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpPut("sent")]
        public async Task<IActionResult> UpdateSentStatus([FromBody] UpdateSentStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.UpdateSentMachine(request.Id, userId);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Machine sent successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.DeleteMachine(id, userId);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Machine deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        public class UpdateSentStatusRequest
        {
            public string Id { get; set; } = string.Empty;
        }
    }

}