using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class MachineAuditService(IMachineAuditRepository machineAuditRepository, IManMachineRepository manMachineRepository, QrTokenService qrTokenService) : IMachineAuditService
    {
        private readonly IMachineAuditRepository _machineAuditRepository = machineAuditRepository;
        private readonly IManMachineRepository _manMachineRepository = manMachineRepository;

        public async Task<Result> CreateMachineAudit(MachineAuditRequest machineAuditRequest)
        {
            if (!await _manMachineRepository.ExistsAsync(machineAuditRequest.MachineId))
            {
                return Result.Failure("Machine does not exist.");
            }

            var result = await qrTokenService.GetUserByQrToken(machineAuditRequest.QrToken);
            string userId = result.UserId;

            var machineAudit = new MachineAudit (machineAuditRequest.MachineId, userId, machineAuditRequest.Action);
            await _machineAuditRepository.AddAudit(machineAudit);

            return Result.Success();
        }
    }
}
