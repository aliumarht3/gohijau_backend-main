using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IWithdrawalHistoryRepository
    {
        Task <string> AddWithdrawalHistory(WithdrawalHistory withdrawalHistory);
        Task <string> AddMachineOwnerWithdrawalHistory(MachineOwnerWithdrawalHistory ownerWithdrawalHistory);
        Task<List<WithdrawalHistory>?> GetWithdrawalHistory(string userId);
        Task<List<MachineOwnerWithdrawalHistory>?> GetWithdrawalHistoryBasedOnOrganizationId(string organizationId);
        Task UpdateWithdrawalStatus(WithdrawalHistory withdrawalHistory);
        Task UpdateMachineOwnerWithdrawalStatus(MachineOwnerWithdrawalHistory withdrawalOwnerHistory);
        Task<WithdrawalHistory?> GetByIdAsync(string id); 
        Task<MachineOwnerWithdrawalHistory?> GetByMachineOwnerIdAsync(string id); 
    }
}
