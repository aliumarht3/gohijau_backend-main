using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IMachineAuditService
    {
        Task<Result> CreateMachineAudit(MachineAuditRequest machineAuditRequest);
    }
}
