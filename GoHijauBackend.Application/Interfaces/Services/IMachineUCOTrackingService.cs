using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IMachineUCOTrackingService
    {
        Task<MachineUCOTracking> CreateNewUCOMachineTracking(string machineId, double bufferVolume);
        public Task<MachineUCOTrackingDTO?> GetMachineUCOTrackingByMachineId(string machineId);
        public  Task<Result<MachineUCOTracking>> UpdateMachineUCOTracking(MachineUCOTrackingDTO uCOTracking);
        public Task<List<MachineUCOTracking>?> GetMachineTrackerByCollectorId(string collectorId);
        public Task<List<MachineUCOTracking>?> GetMachineTrackerByOwnerId(string ownerId);
        public Task<List<MachineUCOTracking>?> GetAllMachineTracker();
    }
}
