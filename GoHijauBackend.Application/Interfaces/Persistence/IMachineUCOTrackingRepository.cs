using CSharpFunctionalExtensions;
using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IMachineUCOTrackingRepository
    {
        Task<bool> CheckMachineExists(string machineId);
        Task<Result<MachineUCOTracking>> AddMachineTrackingBuffer(MachineUCOTracking uCOTracking); 
        Task <Result<MachineUCOTracking>> UpdateUCOTracker (MachineUCOTracking uCOTracking);
        Task<MachineUCOTracking> GetUCOTrackerByMachineId(string machineId);
        Task<List<MachineUCOTracking>> GetUCOTrackerByCollectorId(string collectorId);
        Task<List<MachineUCOTracking>> GetUCOTrackerByOwnerId(string ownerId);
        Task<List<MachineUCOTracking>> GetAllUCOTracker();
    }
}
