using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IOrganizationService
    {
        Task<Result> CreateOrganization(string userId, OrganizationCommand organizationCommand);
        Task<Organization?> GetOrganizationById(string organizationId);
        Task<List<Organization>> GetAllOrganizations();
        Task<List<Organization>> GetAllOilCollectorsOrganizations();
        Task<List<Organization>> GetAllOwnersOrganizations();
        Task<List<Organization>> GetAllTechniciansOrganizations();
        Task<Result> UpdateOrganization(string userId, string organizationId, OrganizationCommand command, IClientSessionHandle? session = null);
        Task<Result> AddOrganizationDebt(string userId, string organizationId, decimal debt);
        Task<Result> UpdateOrganizationOutstandingDebt(string userId, string organizationId, decimal debt, IClientSessionHandle? session = null);
        Task<Result> DeleteOrganization(string organizationId);
        Task<Result> UpdateMachineOwnerRate(string userId, double machineOwnerRate, string organizationId);
        Task<Result> UpdateCollectorRate(string userId, double collectorRate, string organizationId);
        Task<Result> AddOrganizationEmails(string userId, IEnumerable<string>? invoiceEmails, IEnumerable<string>? notificationEmails, string organizationId);
    }
}
