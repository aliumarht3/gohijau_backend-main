using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface ITotalTransactionRepository
    {
        Task UpdateTotalsAsync(string userId, double oilPoured, double co2Saved, double points, string accessToken, IClientSessionHandle session);
        Task<TotalMachineOwnerTransaction?> UpdateMachineOwnerTotalsAsync(string organizationId, double oilPoured, double co2Saved, double points, string accessToken, IClientSessionHandle session);
        Task UpdateCollectorTotalsAsync(string userId, double oilCollected, double co2Saved, string accessToken, IClientSessionHandle session);
        Task<TotalTransaction?> GetByUserIdAsync(string userId);
        Task<TotalMachineOwnerTransaction?> GetByOrganizationIdAsync(string organizationId);
        Task<decimal> GetCustomerTotalPointsAwarded(string userId);
        Task<bool> DeductCustomerTotalPoint(string userId, double points);
        Task<decimal> GetMachineOwnerTotalPointsAwarded(string organizationId);
        Task<decimal> GetAllMachineOwnersTotalPointsAwarded();
        Task<bool> DeductMachineOwnerTotalPoint(string userId, double points);
        Task<decimal> GetAllCustomersTotalPointsAwarded();
    }
}
