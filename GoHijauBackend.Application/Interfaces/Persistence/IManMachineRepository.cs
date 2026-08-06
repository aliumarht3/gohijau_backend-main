using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IManMachineRepository
    {
        Task AddAsync(ManMachine manMachine);
        Task<bool> ExistsAsync(string machineId);
        Task<ManMachine?> GetByIdAsync(string id);
        Task<List<ManMachine>> GetAllMachinesAsync();
        Task UpdateAsync(ManMachine manMachine);
        Task<bool> DeleteAsync(string id);
    }
}
