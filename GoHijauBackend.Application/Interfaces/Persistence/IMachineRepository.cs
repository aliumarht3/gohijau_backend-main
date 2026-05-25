using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IMachineRepository
    {
        Task AddAsync(Machine machine);
        Task<Machine?> GetByIdAsync(string id);
        Task<Machine?> GetByMachineIdAsync(string id);
        Task<List<Machine>> GetAllMachinesAsync();
        Task<List<Machine>> GetAllOwnerMachinesAsync(string userId);
        Task<List<Machine>> GetMachinesByCollectorOrganizationId(string organizationId);
        Task<bool> ExistsAsync(string machineId);
        Task UpdateAsync(Machine machine);
        Task<List<Machine>> SearchMachinesAsync(MachineSearchRequest request);
        Task<bool> DeleteAsync(string id);
    }
}
