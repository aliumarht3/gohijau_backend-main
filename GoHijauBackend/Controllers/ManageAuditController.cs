using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/audit")]
    public class ManageAuditController : ControllerBase
    {
        private readonly IMachineAuditService _service;
        public ManageAuditController(IMachineAuditService service)
        {
            _service = service;
        }

        [HttpPost("machine/create")]
        public async Task<IActionResult> Create([FromBody] MachineAuditRequest machineAuditRequest)
        {
            try
            {
                var result = await _service.CreateMachineAudit(machineAuditRequest);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Audit Created Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
