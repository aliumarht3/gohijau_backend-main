using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Hubs;
using GoHijauBackend.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GoHijauBackend.Controllers
{
    [ApiController]
    [Route("api/machine")]
    public class MachineTelemetryController : ControllerBase
    {
        private readonly LiveMachineCache _cache;
        private readonly IHubContext<MachineHub> _hubContext;

        public MachineTelemetryController(LiveMachineCache cache, IHubContext<MachineHub> hubContext)
        {
            _cache = cache;
            _hubContext = hubContext;
        }

        [HttpPost("telemetry")]
        public async Task<IActionResult> ReceiveTelemetry([FromBody] IncomingPythonTelemetry payload)
        {
            var telemetry = new MachineTelemetry
            {
                MachineId = payload.MachineId,
                IsOnline = true,
                Metrics = payload.Metrics
            };

            _cache.Machines[payload.MachineId] = telemetry;
            await _hubContext.Clients.All.SendAsync("ReceiveTelemetryUpdate", telemetry);

            return Ok(new { message = "Telemetry saved and broadcasted." });
        }

        [HttpGet("telemetry")]
        public IActionResult GetTelemetry()
        {
            return Ok(_cache.Machines.Values.ToList());
        }
    }
}