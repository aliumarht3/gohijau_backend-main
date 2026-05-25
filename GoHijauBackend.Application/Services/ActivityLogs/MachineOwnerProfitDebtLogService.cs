using CSharpFunctionalExtensions;
using GoHijauBackend.Domain.Entities;
using GoHijauBackend.Application.Interfaces.Persistence.ActivityLogs;
using GoHijauBackend.Application.Interfaces.Services.ActivityLogs;
using GoHijauBackend.Domain.Enum;
using GoHijauBackend.Infrastructure.Persistence.ActivityLogs;
using MongoDB.Driver;

namespace GoHijauBackend.Application.Services.ActivityLogs
{
    public class MachineOwnerProfitDebtLogService : IMachineOwnerProfitDebtLogService
    {
        private readonly IMachineOwnerProfitDebtLogRepository _machineOwnerProfitDebtLogRepository;
        public MachineOwnerProfitDebtLogService(IMachineOwnerProfitDebtLogRepository machineOwnerProfitDebtLogRepository)
        {
            _machineOwnerProfitDebtLogRepository = machineOwnerProfitDebtLogRepository;
        }
        public async Task<Result> AddDebtLog(string userId, MachineOwnerLogCommand machineOwnerLogCommand, IClientSessionHandle session)
        {
            return await CreateLog(
            userId,
            machineOwnerLogCommand,
            MachineOwnerProfitLogType.ADD_DEBT,
            "Add Debt Log",
            session);
        }

        public async Task<Result> ReduceDebtLog(string userId, MachineOwnerLogCommand machineOwnerLogCommand, IClientSessionHandle session)
        {
            return await CreateLog(
            userId,
            machineOwnerLogCommand,
            MachineOwnerProfitLogType.REDUCE_DEBT,
            "Reduce Debt Log",
            session);
        }

        public async Task<Result> AddEwalletLog(string userId, MachineOwnerLogCommand machineOwnerLogCommand, IClientSessionHandle session)
        {
            return await CreateLog(
            userId,
            machineOwnerLogCommand,
            MachineOwnerProfitLogType.ADD_EWALLET,
            "Add EWallet Log",
            session);
        }

        public async Task<Result> CreateLog(string userId, MachineOwnerLogCommand machineOwnerLogCommand, MachineOwnerProfitLogType type, string operationName, IClientSessionHandle session)
        {
            try
            {
                var machineOwnerProfitDebtLog = new MachineOwnerProfitDebtLog
                {
                    CustomerTransactionId = machineOwnerLogCommand.CustomerTransactionId,
                    MachineOwnerOrganizationId = machineOwnerLogCommand.MachineOwnerOrganizationId,
                    MachineId = machineOwnerLogCommand.MachineId,
                    CustomerId = machineOwnerLogCommand.CustomerId,
                    UcoWeight = machineOwnerLogCommand.UcoWeight,
                    ProfitRate = machineOwnerLogCommand.ProfitRate,
                    Amount = machineOwnerLogCommand.Amount,
                    Type = type,
                    NewEwalletBalance = machineOwnerLogCommand.NewEwalletBalance,
                    NewDebtBalance = machineOwnerLogCommand.NewDebtBalance,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _machineOwnerProfitDebtLogRepository.InsertLog(machineOwnerProfitDebtLog, session);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to {operationName}: {ex.Message}");
            }
        }
    }
}
