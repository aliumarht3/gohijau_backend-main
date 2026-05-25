using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;


namespace GoHijauBackend.Application.Services
{
    public class ManMachineService(IManMachineRepository manMachineRepository, IMachineService machineService) : IManMachineService
    {
        private readonly IManMachineRepository _manMachineRepository = manMachineRepository;
        private readonly IMachineService _machineService = machineService;
        public async Task<Result> ManufactureMachine(string userId, ManMachineDTO manMachineDTO)
        {
            if (await _manMachineRepository.ExistsAsync(manMachineDTO.MachineId))
            {
                return Result.Failure("MachineId already exists.");
            }

            var machine = new ManMachine(manMachineDTO.MachineId, manMachineDTO.Status, userId);
            await _manMachineRepository.AddAsync(machine);

            return Result.Success();
        }

        public async Task<Result> DeleteMachine(string id, string userId)
        {
            var machine = await _manMachineRepository.GetByIdAsync(id);
            if (machine == null)
                return Result.Failure($"Machine with id {id} not found");

            machine.Status = ManMachineStatus.DELETED;
            machine.ModifiedAt = DateTime.UtcNow;
            machine.ModifiedBy = userId;

            await _manMachineRepository.UpdateAsync(machine);

            return Result.Success();
        }

        public async Task<List<ManMachine>> GetAllMachines()
        {
            return await _manMachineRepository.GetAllMachinesAsync();
        }

        public async Task<List<ManMachine>> GetAllUnassignedMachines()
        {
            var assignedMachines = await _machineService.GetAllMachines();
            // Extract their IDs
            var assignedIds = assignedMachines.Select(m => m.MachineId).ToHashSet();

            // Get all machines from repository
            var allMachines = await _manMachineRepository.GetAllMachinesAsync();

            // Exclude assigned ones
            var unassignedMachines = allMachines
                .Where(m => !assignedIds.Contains(m.MachineId))
                .ToList();

            return unassignedMachines;
        }

        public async Task<Result> UpdateSentMachine(string id, string userId)
        {
            var machine = await _manMachineRepository.GetByIdAsync(id);
            if (machine == null)
                return Result.Failure($"Machine with id {id} not found");
            machine.Status = ManMachineStatus.SENT;
            machine.ModifiedAt = DateTime.UtcNow;
            machine.ModifiedBy = userId;

            await _manMachineRepository.UpdateAsync(machine);

            return Result.Success();
        }
    }
}
