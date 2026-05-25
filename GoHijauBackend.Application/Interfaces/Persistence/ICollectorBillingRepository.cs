using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface ICollectorBillingRepository
    {
        Task<List<string>> GetUserIdsByOrganizationAsync(string organizationId);
        Task<List<CollectorTransaction>> GetCollectorTransactionsByUserIdsAsync(List<string> userIds);
        Task<List<RazorPayOrder>> GetSuccessfulOrdersByUserIdsAsync(List<string> userIds);
        Task<Organization?> GetOrganizationByIdAsync(string organizationId);
    }
}
