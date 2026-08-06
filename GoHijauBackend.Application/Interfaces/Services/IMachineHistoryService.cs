using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IMachineHistoryService
    {
        Task<List<DepositionHistoryDto>> GetDepositionHistoryByMachineIdAsync(string machineId);
        Task<List<CollectionHistoryDto>> GetCollectionHistoryByMachineIdAsync(string machineId);
    }
}
