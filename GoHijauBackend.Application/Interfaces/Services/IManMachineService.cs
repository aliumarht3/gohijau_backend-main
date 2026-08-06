using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IManMachineService
    {
        Task<List<ManMachine>> GetAllMachines();
        Task<List<ManMachine>> GetAllUnassignedMachines();
        Task<Result> ManufactureMachine(string userId, ManMachineDTO manMachineDTO);
        Task<Result> UpdateSentMachine(string id, string userId);
        Task<Result> DeleteMachine(string id, string userId);
    }
}
