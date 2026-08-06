using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IMachineService
    {
        //Task<User?> GetUserById(string id);
        Task<List<Machine>> GetAllMachines();
        Task<List<Machine>> GetAllOwnerMachines(string userId);
        Task<List<Machine>> GetMachinesByCollectorUserId(string userId);
        Task<Result> CreateMachine(string userId, MachineCommand machineCommand);
        Task<Result> UpdateMachine(string id, string userId, MachineCommand machineCommand);
        Task<List<Machine>> SearchMachines(MachineSearchRequest request);
        Task<Result> DeleteMachine(string id, string userId);
    }
}
