using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface ICustomerBankAccountRepository
    {
        Task CreateOrUpdateBankAccount(BankAccount bankAccount);
        Task <BankAccount?> GetBankAccount(String userId);
    }
}
