using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoCustomerBankAccountRepository : ICustomerBankAccountRepository
    {
        private readonly IMongoCollection<BankAccount> _bankAccounts;

        public MongoCustomerBankAccountRepository(IMongoDatabase database)
        {
            _bankAccounts = database.GetCollection<BankAccount>("CustomerBankAccounts");
        }

        public async Task CreateOrUpdateBankAccount(BankAccount bankAccount)
        {
            var filter = Builders<BankAccount>.Filter.Eq(x => x.CustomerId, bankAccount.CustomerId);

            var update = Builders<BankAccount>.Update
                .Set(x => x.BankCode, bankAccount.BankCode)
                .Set(x => x.AccountNumber, bankAccount.AccountNumber)
                .Set(x => x.ModifiedAt, DateTime.UtcNow)
                .Set(x => x.ModifiedBy, bankAccount.ModifiedBy)
                .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow)
                .SetOnInsert(x => x.CreatedBy, bankAccount.CreatedBy);

            var options = new UpdateOptions { IsUpsert = true };

            await _bankAccounts.UpdateOneAsync(filter, update, options);
        }

        async Task<BankAccount?> ICustomerBankAccountRepository.GetBankAccount(string userId)
        {
            BankAccount ba = new BankAccount();
            ba = await _bankAccounts.Find(x => x.CustomerId == userId).FirstOrDefaultAsync();

            return ba; 
        }
    }
}
