using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IOrganizationRepository
    {
        Task AddAsync(Organization organization);
        Task<List<Organization>> GetAllOrganizationsAsync();
        Task<List<Organization>> GetOrganizationsByIds(List<string> organizationIds);
        Task<Organization> GetOrganizationById(string organizationId);
        Task UpdateMachineOwnerRate(double machineOwnerRate,string organizationId,string userId);
        Task UpdateCollectorRate(double collectorRate, string organizationId, string userId);
        Task AddOrganizationEmails(IEnumerable<string> invoiceEmails, IEnumerable<string> notificationEmails, string organizationId, string userId);
        Task UpdateOrganizationAsync(string organizationId, Organization organization, IClientSessionHandle? session = null);
        Task<bool> AddOrganizationTotalNOutstandingDebt(string organizationId, double debt, string modifiedBy, DateTime modifiedAt, IClientSessionHandle? session = null);
        Task<bool> UpdateOrganizationOutstandingDebt(string organizationId, double debt, string modifiedBy, DateTime modifiedAt, IClientSessionHandle? session = null);
        Task DeleteOrganizationAsync(string organizationId);
    }
}
