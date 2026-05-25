using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class MachineService(IMachineRepository machineRepository, IUserRepository userRepository) : IMachineService
    {
        private readonly IMachineRepository _machineRepository = machineRepository;

        private readonly IUserRepository _userRepository = userRepository;

        public async Task<List<Machine>> GetAllMachines()
        {
            return await _machineRepository.GetAllMachinesAsync();
        }
        public async Task<List<Machine>> GetAllOwnerMachines(string userId)
        {
            return await _machineRepository.GetAllOwnerMachinesAsync(userId);
        }

        public async Task<List<Machine>> GetMachinesByCollectorUserId(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.OrganizationId))
                return new List<Machine>();

            return await _machineRepository.GetMachinesByCollectorOrganizationId(user.OrganizationId);
        }
        public async Task<Result> CreateMachine(string userId, MachineCommand machineCommand)
        {
            if (await _machineRepository.ExistsAsync(machineCommand.MachineId))
            {
                return Result.Failure("MachineId already exists.");
            }

            var location = new Location(
                machineCommand.Location.Name,
                machineCommand.Location.UnitNo,
                machineCommand.Location.Street,
                machineCommand.Location.District,
                machineCommand.Location.Postcode,
                machineCommand.Location.State,
                machineCommand.Location.Country,
                machineCommand.Location.Coordinates
            );

            var machine = new Machine(machineCommand.MachineId, location, machineCommand.Type, machineCommand.ManufacturedDate, machineCommand.Status, machineCommand.Owner, machineCommand.Collector, machineCommand.Technician, userId);
            await _machineRepository.AddAsync(machine);

            return Result.Success();
        }

        public async Task<Result> UpdateMachine(string id, string userId, MachineCommand machineCommand)
        {
            var machine = await _machineRepository.GetByIdAsync(id);
            if (machine == null)
                return Result.Failure($"Machine with id {id} not found");

            machine.Location = new Location(
                machineCommand.Location.Name,
                machineCommand.Location.UnitNo,
                machineCommand.Location.Street,
                machineCommand.Location.District,
                machineCommand.Location.Postcode,
                machineCommand.Location.State,
                machineCommand.Location.Country,
                machineCommand.Location.Coordinates
            );
            machine.Type = machineCommand.Type;
            machine.Status = machineCommand.Status;
            machine.Owner = machineCommand.Owner;
            machine.Collector = machineCommand.Collector;
            machine.Technician = machineCommand.Technician;
            machine.ModifiedAt = DateTime.UtcNow;
            machine.ModifiedBy = userId;

            await _machineRepository.UpdateAsync(machine);

            return Result.Success();
        }

        public async Task<List<Machine>> SearchMachines(MachineSearchRequest request)
        {
            return await _machineRepository.SearchMachinesAsync(request);
        }

        public async Task<Result> DeleteMachine(string id, string userId)
        {
            var machine = await _machineRepository.GetByIdAsync(id);
            if (machine == null)
                return Result.Failure($"Machine with id {id} not found");

            if (machine.Status == MachineStatus.DELETED)
                return Result.Failure("Machine is already deleted.");

            machine.Status = MachineStatus.DELETED;
            machine.ModifiedAt = DateTime.UtcNow;
            machine.ModifiedBy = userId;

            await _machineRepository.UpdateAsync(machine);

            return Result.Success();
        }
    }
}
