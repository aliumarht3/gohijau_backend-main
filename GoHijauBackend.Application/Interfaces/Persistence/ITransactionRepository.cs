using MongoDB.Driver;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface ITransactionRepository
    {
        Task<Transaction> InsertTransactionAsync(Transaction transaction, IClientSessionHandle session);
        Task InsertCollectorTransactionAsync(CollectorTransaction transaction);
        Task UpdateCollectorTransactionAsync(CollectorTransaction transaction, IClientSessionHandle session);
        Task<List<Transaction>> GetByUserIdAsync(string user_id);
        Task<List<Transaction>> GetByMachineIdAsync(string user_id);
        Task<double> GetAdminTotalOilPoured();
        Task<double> GetOwnerTotalOilPoured(string user_id);
        Task<List<CollectorTransaction>> GetTransactionsByCollectorId(string user_id);
        Task<List<CollectorTransaction>> GetCollectorTransactionsByMachineIdAsync(string machineId);
        Task<List<CollectorTransaction>> GetCollectorTransactionsByUserIdsAsync(List<string> userIds);
        Task<Transaction?> GetTransactionsByAccessTokenAsync(string accessToken);
        Task<CollectorTransaction?> GetCollectorTransactionsByAccessTokenAsync(string accessToken);
    }
}
