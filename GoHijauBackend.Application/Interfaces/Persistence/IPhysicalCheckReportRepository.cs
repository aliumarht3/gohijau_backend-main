using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IPhysicalCheckReportRepository
    {
        Task CreateReportAsync(PhysicalCheckReport report);
        Task<List<PhysicalCheckReport>> GetReportsByMachineIdAsync(string machineId);
    }
}