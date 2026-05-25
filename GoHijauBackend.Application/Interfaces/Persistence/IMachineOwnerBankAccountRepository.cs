using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IMachineOwnerBankAccountRepository
    {
        Task CreateOrUpdateBankAccount(BankAccount bankAccount);
        Task<BankAccount?> GetBankAccount(String organizationId);
    }
}
