using GoHijauBackend.Infrastructure.Persistence.ActivityLogs;
using MongoDB.Driver;

namespace GoHijauBackend.Application.Interfaces.Persistence.ActivityLogs
{
    public interface IMachineOwnerProfitDebtLogRepository
    {
        Task InsertLog(MachineOwnerProfitDebtLog log, IClientSessionHandle session);
    }
}
