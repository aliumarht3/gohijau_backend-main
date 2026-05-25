using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/machine")]
    public class ManageMachineController : ControllerBase
    {
        private readonly IMachineService _machineService;
        private readonly IManMachineService _manMachineService;
        public ManageMachineController(IMachineService machineService, IManMachineService manMachineService)
        {
            _machineService = machineService;
            _manMachineService = manMachineService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var machines = await _machineService.GetAllMachines();

                return Ok(machines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpGet("owner")]
        public async Task<IActionResult> GetAllOwnerMachines()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
                var machines = await _machineService.GetAllOwnerMachines(userId);

                return Ok(machines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("collector")]
        public async Task<IActionResult> GetMachinesByCollectorUserId()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var machines = await _machineService.GetMachinesByCollectorUserId(userId);

                return Ok(machines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("unassigned")]
        public async Task<IActionResult> GetAllUnassigned()
        {
            try
            {
                var unassignedMachines = await _manMachineService.GetAllUnassignedMachines();

                return Ok(unassignedMachines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] MachineCommand machineCommand)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _machineService.CreateMachine(userId, machineCommand);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Machine Created Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPatch("update/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MachineCommand machineCommand)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _machineService.UpdateMachine(id, userId, machineCommand);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Machine Updated Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] MachineSearchRequest request)
        {
            try
            {
                var machines = await _machineService.SearchMachines(request);
                return Ok(machines);
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
                var result = await _machineService.DeleteMachine(id, userId);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Machine deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
