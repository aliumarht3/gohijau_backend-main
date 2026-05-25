using AutoMapper;
using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Hubs;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.SignalR;


namespace GoHijauBackend.Application.Services
{
    public class MachineUCOTrackingService(IMachineUCOTrackingRepository uCOTrackingRepository, IMachineRepository machineRepository, 
        IHubContext<MachineHub> hubContext, IMapper mapper, ICollectorUCOReminder collectorUCOReminder) : IMachineUCOTrackingService
    {
        private readonly ICollectorUCOReminder _collectorUCOReminder = collectorUCOReminder;
        private readonly IMachineUCOTrackingRepository _repository = uCOTrackingRepository;
        private readonly IMachineRepository _repositoryMachine = machineRepository;
        private readonly IHubContext<MachineHub> _hubContext = hubContext;
        private readonly IMapper _mapper = mapper;

        private static string MachineGroup(string machineId) => $"machine:{machineId}";
        public async Task<MachineUCOTracking?> CreateNewUCOMachineTracking(
      string machineId,
      double bufferVolume)
        {
            var resultMachine = await _repositoryMachine.GetByMachineIdAsync(machineId);

            var ucoTrackingEntity = new MachineUCOTracking
            {
                MachineId = machineId,
                CreatedAt = DateTime.UtcNow,
                BufferVolume = bufferVolume,
                MachineLocationName = resultMachine?.Location?.Name
            };

            bool checkMachineExists = await _repository.CheckMachineExists(machineId);
            if (checkMachineExists)
            {
                return null;
            }

            var result = await _repository.AddMachineTrackingBuffer(ucoTrackingEntity);

            if (result.IsFailure)
            {
                throw new Exception(result.Error); // FIXED
            }

            return result.Value; // or result.Data if you named it that way
        }


        public async Task<Result<MachineUCOTracking>> UpdateMachineUCOTracking(MachineUCOTrackingDTO uCOTracking) {

            try
            {
                var resultMachine = await _repositoryMachine.GetByMachineIdAsync(uCOTracking.MachineId);
                var ucoTrackingEntity = new MachineUCOTracking();
                ucoTrackingEntity.Id = uCOTracking.Id; 
                ucoTrackingEntity.MachineLocationName = uCOTracking.MachineLocationName;
                ucoTrackingEntity.MachineId = uCOTracking.MachineId;
                ucoTrackingEntity.BufferVolume = uCOTracking.BufferVolume;
                ucoTrackingEntity.ModifiedAt = DateTime.UtcNow;
                bool checkMachineExists = await _repository.CheckMachineExists(uCOTracking.MachineId);
                if (checkMachineExists) 
                {
                    var result = await _repository.UpdateUCOTracker(ucoTrackingEntity);
                    if (result.IsSuccess) {
                        await _hubContext.Clients
                             .Group(MachineGroup(ucoTrackingEntity.MachineId))
                             .SendAsync("ReceiveMachineVolumeUpdate", ucoTrackingEntity);
                        var percentage = (result.Value.BufferVolume / 120)*100;
                        if (percentage >= 80 && !resultMachine.Sent80PercentReminder) 
                        {
                            resultMachine.Sent80PercentReminder = true;
                               
                            Reminder reminder = new Reminder();
                            reminder.MachineId = ucoTrackingEntity.MachineId;
                            reminder.Message = percentage.ToString();
                           await _collectorUCOReminder.SendReminder(reminder);

                        }
                        if (percentage >= 100 && !resultMachine.Sent100PercentReminder)
                        {
                            resultMachine.Sent100PercentReminder = true;
                            Reminder reminder = new Reminder();
                            reminder.MachineId = ucoTrackingEntity.MachineId;
                            reminder.Message = "100";
                            await _collectorUCOReminder.SendReminder(reminder);

                        }
                        if (ucoTrackingEntity.BufferVolume == 0) 
                        {
                            resultMachine.Sent80PercentReminder = false;
                            resultMachine.Sent100PercentReminder = false;  
                        }
                        await _repositoryMachine.UpdateAsync(resultMachine);

                        return Result.Success<MachineUCOTracking>(result.Value) ;
                    }
                    else
                    {
                        return Result.Failure<MachineUCOTracking>("Error updating machine buffer");
                    }
                }
                return Result.Failure<MachineUCOTracking>("Machine doesn't exist");
            }
            catch (Exception ex) 
            {
                return Result.Failure<MachineUCOTracking>(ex.Message);
            }
        }
        public async Task<List<MachineUCOTracking>?> GetMachineTrackerByCollectorId(string collectorId) 
        {
            var result = await _repository.GetUCOTrackerByCollectorId(collectorId);
            return result?.ToList();  
        }
        public async Task<List<MachineUCOTracking>?> GetMachineTrackerByOwnerId(string ownerId) 
        {
            var result = await _repository.GetUCOTrackerByOwnerId(ownerId);
            return result?.ToList();  
        }

        public async Task<MachineUCOTrackingDTO?> GetMachineUCOTrackingByMachineId(string machineId)
        {
           var result = await _repository.CheckMachineExists(machineId);
            if (result) 
            {
                var machineUcoTracking  = await _repository.GetUCOTrackerByMachineId(machineId);
                return _mapper.Map<MachineUCOTrackingDTO>(machineUcoTracking);
            }
            return null;
        }

        public async Task<List<MachineUCOTracking>?> GetAllMachineTracker()
        {
            var result = await _repository.GetAllUCOTracker();
            return result?.ToList();
        }
    }
}
