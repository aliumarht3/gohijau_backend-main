using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
public class BankAccountService(ICustomerBankAccountRepository customerBankAccountRepository, IMachineOwnerBankAccountRepository machineOwnerBankAccountRepository, IUserRepository userRepository ) : IBankAccountService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ICustomerBankAccountRepository _customerBankAccountRepository = customerBankAccountRepository;
        private readonly IMachineOwnerBankAccountRepository _machineOwnerBankAccountRepository = machineOwnerBankAccountRepository;
        public async Task<(bool IsSuccess, string? Error)> CreateOrUpdateBankAccount(string userId, CreateBankAccountRequest request)
        {
            try
            {
                var bankAccount = new BankAccount
                {
                    CustomerId = userId,
                    BankCode = request.BankCode,
                    AccountNumber = request.AccountNumber,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _customerBankAccountRepository.CreateOrUpdateBankAccount(bankAccount);
                return (true, null);
            }
            catch
            {
                return (false, "Something went wrong. Please try again");
            }
        }

        public async Task<BankAccount?> GetBankAccount(string userId)
        {
          return  await _customerBankAccountRepository.GetBankAccount(userId);
        }

        public async Task<(bool IsSuccess, string? Error)> CreateOrUpdateMachineOwnerBankAccount(string userId, CreateBankAccountRequest request)
        {
            try
            {
                var user =await  _userRepository.GetByIdAsync(userId);
                var bankAccount = new BankAccount
                {
                    OrganizationId = user.OrganizationId,
                    BankCode = request.BankCode,
                    AccountNumber = request.AccountNumber,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _machineOwnerBankAccountRepository.CreateOrUpdateBankAccount(bankAccount);
                return (true, null);
            }
            catch
            {
                return (false, "Something went wrong. Please try again");
            }
        }

        public async Task<BankAccount?> GetMachineOwnerBankAccount(string organizationId)
        {
            return await _machineOwnerBankAccountRepository.GetBankAccount(organizationId);
        }
    }
}
