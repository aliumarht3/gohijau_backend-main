using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IWithdrawalHistoryService
    {
        Task<(bool IsSuccess, string? Error, string Id)> AddWithdrawalHistory(string amount, string userId);
        Task<(bool IsSuccess, string? Error, string Id)> AddMachineOwnerWithdrawalHistory(string organizationId,string amount, string userId);
        Task<(bool IsSuccess, string? Error)> UpdateWithdrawalStatus(string withdrawalId, string status);
        Task<(bool IsSuccess, string? Error)> UpdateMachineOwnerWithdrawalStatus(string withdrawalId, string status);
        Task<List<WithdrawalHistory>?> GetWithdrawalHistory(string userId);
        Task<List<MachineOwnerWithdrawalHistory>?> GetWithdrawalHistoryBasedOnOrganizationId(string organizationId);
    }
}
