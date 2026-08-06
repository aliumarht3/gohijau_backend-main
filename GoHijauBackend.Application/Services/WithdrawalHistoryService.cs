using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class WithdrawalHistoryService(IWithdrawalHistoryRepository withdrawalHistoryRepository,IWithdrawalNotifierService notifier,IUserService userService) : IWithdrawalHistoryService
    {
        private readonly IWithdrawalHistoryRepository _withdrawalHistoryRepository = withdrawalHistoryRepository;
        private readonly IWithdrawalNotifierService _notifier = notifier;
        private readonly IUserService _userService = userService;
        public async Task<(bool IsSuccess, string? Error, string Id)> AddWithdrawalHistory(string amount, string userId)
        {
            try {
                var history = new WithdrawalHistory
                {
                    Amount = amount,
                    CustomerId = userId,
                    Status = "IN PROGRESS",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
               var result = await _withdrawalHistoryRepository.AddWithdrawalHistory (history);
                return (true, null,result);
            } 
            catch (Exception e) {

                return (false, "Something went wrong. Please try again","ID could not be fetched");
            }
        }
        public async Task<(bool IsSuccess, string? Error, string Id)> AddMachineOwnerWithdrawalHistory(string organizationId,string amount,string userId)
        {
            try {
                var history = new MachineOwnerWithdrawalHistory
                {
                    Amount = amount,
                    OrganizationId = organizationId,
                    Status = "IN PROGRESS",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
               var result = await _withdrawalHistoryRepository.AddMachineOwnerWithdrawalHistory (history);
                return (true, null,result);
            } 
            catch (Exception e) {

                return (false, "Something went wrong. Please try again","ID could not be fetched");
            }
        }
        public async Task<List<WithdrawalHistory>?> GetWithdrawalHistory(string userId)
        {
               var result =  await _withdrawalHistoryRepository.GetWithdrawalHistory(userId);
                return result; 
        }

        public async Task<List<MachineOwnerWithdrawalHistory>?> GetWithdrawalHistoryBasedOnOrganizationId(string organizationId)
        {
            var result = await _withdrawalHistoryRepository.GetWithdrawalHistoryBasedOnOrganizationId(organizationId);
            return result;
        }

        public async Task<(bool IsSuccess, string? Error)> UpdateWithdrawalStatus(string withdrawalId, string status)
        {
            try 
            {
                if (string.IsNullOrWhiteSpace(withdrawalId))
                    return (false, "Invalid withdrawalId");
                WithdrawalHistory? wh = await _withdrawalHistoryRepository.GetByIdAsync(withdrawalId);
                if (wh is null)
                    return (false, "Withdrawal not found");
                wh.Status = status;
                wh.ModifiedAt = DateTime.UtcNow; 
                await _withdrawalHistoryRepository.UpdateWithdrawalStatus(wh);
                await _notifier.NotifyChangedAsync(wh.CustomerId, wh);
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, "Something went wrong. Please try again");
            }
        }

        public async Task<(bool IsSuccess, string? Error)> UpdateMachineOwnerWithdrawalStatus(string withdrawalId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(withdrawalId))
                    return (false, "Invalid withdrawalId");
                MachineOwnerWithdrawalHistory? wh = await _withdrawalHistoryRepository.GetByMachineOwnerIdAsync(withdrawalId);
                if (wh is null)
                    return (false, "Withdrawal not found");
                wh.Status = status;
                wh.ModifiedAt = DateTime.UtcNow;
                await _withdrawalHistoryRepository.UpdateMachineOwnerWithdrawalStatus(wh);
                List<User> users = await _userService.GetUserFromOrganizationId(wh.OrganizationId);
                await _notifier.NotifyChangedOwnerAsync(users, wh);
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, "Something went wrong. Please try again");
            }
        }
    }
}

