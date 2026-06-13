using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Hubs;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GoHijauBackend.Controllers
{
    [ApiController]
    [Route("api/machine")]
    public class MachineDiagnosticsController : ControllerBase
    {
        private readonly LiveMachineCache _cache;
        private readonly IHubContext<MachineHub> _hubContext;
        private readonly IPhysicalCheckReportRepository _reportRepository;

        public MachineDiagnosticsController(
            LiveMachineCache cache, 
            IHubContext<MachineHub> hubContext,
            IPhysicalCheckReportRepository reportRepository)
        {
            _cache = cache;
            _hubContext = hubContext;
            _reportRepository = reportRepository;
        }

        [HttpPost("diagnostics")]
        public async Task<IActionResult> ReceiveDiagnostic([FromBody] DiagnosticLogDto log)
        {
            if (!_cache.DiagnosticsStore.ContainsKey(log.MachineId))
            {
                _cache.DiagnosticsStore[log.MachineId] = new List<DiagnosticLogDto>();
            }

            var machineLogs = _cache.DiagnosticsStore[log.MachineId];
            var existing = machineLogs.FirstOrDefault(l => l.Component == log.Component);
            
            if (existing != null)
            {
                existing.Status = log.Status;
                existing.Action = log.Action;
                existing.Timestamp = log.Timestamp;
            }
            else
            {
                machineLogs.Add(log);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveDiagnosticLog", log);
            return Ok(new { message = "Log received and broadcasted." });
        }

    [HttpPost("{machineId}/trigger-online")]
    public async Task<IActionResult> TriggerOnline(string machineId)
    {
        // Override the database mismatch
        if (machineId == "PERLIS-01") 
        {
            machineId = "GO-000001";
        }

        await _hubContext.Clients.All.SendAsync("RunOnlineDiagnostics", machineId);
        return Ok(new { message = "Startup diagnostic command sent to machine." });
    }

    [HttpPost("{machineId}/trigger-physical/{component}")]
    public async Task<IActionResult> TriggerPhysical(string machineId, string component)
    {
        // Override the database mismatch
        if (machineId == "PERLIS-01") 
        {
            machineId = "GO-000001";
        }

        await _hubContext.Clients.All.SendAsync("RunPhysicalDiagnostics", machineId, component);
        return Ok(new { message = $"Test command for {component} sent to machine." });
    }

        [HttpPost("physical-checks")]
        public async Task<IActionResult> SubmitPhysicalCheck([FromBody] PhysicalCheckReport report)
        {
            report.Timestamp = DateTime.UtcNow;
            await _reportRepository.CreateReportAsync(report);
            return Ok(new { message = "Physical check report saved to MongoDB successfully." });
        }

        [HttpGet("physical-checks/{machineId}")]
        public async Task<IActionResult> GetPhysicalChecks(string machineId)
        {
            var reports = await _reportRepository.GetReportsByMachineIdAsync(machineId);
            return Ok(reports);
        }

        [HttpGet("errors")]
        public IActionResult GetErrors() => Ok(new List<object>());
    }
}