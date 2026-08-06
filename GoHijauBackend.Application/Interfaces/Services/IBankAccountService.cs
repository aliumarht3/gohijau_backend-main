using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IBankAccountService
    {
        Task<(bool IsSuccess, string? Error)> CreateOrUpdateBankAccount(string userId, CreateBankAccountRequest request);
        Task<BankAccount?> GetBankAccount(string userId);
        Task<BankAccount?> GetMachineOwnerBankAccount(string organizationId);
        Task<(bool IsSuccess, string? Error)> CreateOrUpdateMachineOwnerBankAccount(string userId, CreateBankAccountRequest request);
    }
}
