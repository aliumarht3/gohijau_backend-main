using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IMachineAuditRepository
    {
        Task AddAudit(MachineAudit machineAudit);
    }
}
