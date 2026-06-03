using GoHijauBackend.Application.Dto;
using System.Collections.Concurrent;

namespace GoHijauBackend.Application.Services
{
    public class LiveMachineCache
    {
        public ConcurrentDictionary<string, MachineTelemetry> Machines { get; } = new();
        public ConcurrentDictionary<string, List<DiagnosticLogDto>> DiagnosticsStore { get; } = new();
    }
}