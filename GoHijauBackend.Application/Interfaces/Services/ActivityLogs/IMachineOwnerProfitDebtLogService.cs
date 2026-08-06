using CSharpFunctionalExtensions;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Application.Interfaces.Services.ActivityLogs
{
    public interface IMachineOwnerProfitDebtLogService
    {
        Task<Result> AddDebtLog(string userId, MachineOwnerLogCommand machineOwnerLogCommand, IClientSessionHandle session);
        Task<Result> ReduceDebtLog(string userId, MachineOwnerLogCommand machineOwnerLogCommand, IClientSessionHandle session);
        Task<Result> AddEwalletLog(string userId, MachineOwnerLogCommand machineOwnerLogCommand, IClientSessionHandle session);
    }
}
