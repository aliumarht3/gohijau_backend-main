using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoWithdrawalHistoryRepository : IWithdrawalHistoryRepository
    {
        private readonly IMongoCollection<WithdrawalHistory> _withdrawalHistory;
        private readonly IMongoCollection<MachineOwnerWithdrawalHistory> _withdrawalOwnerHistory;
        private readonly IMongoCollection<User> _users;

        public MongoWithdrawalHistoryRepository(IMongoDatabase database) {
            _withdrawalHistory = database.GetCollection<WithdrawalHistory>("CustomerWithdrawalHistories");
            _withdrawalOwnerHistory = database.GetCollection<MachineOwnerWithdrawalHistory>("MachineOwnerWithdrawalHistories");
            _users = database.GetCollection<User>("users");
        }
        public async Task<string> AddWithdrawalHistory(WithdrawalHistory withdrawalHistory)
        {
            await _withdrawalHistory.InsertOneAsync(withdrawalHistory);
            return withdrawalHistory.Id; 
        }

        public async Task<List<WithdrawalHistory>?> GetWithdrawalHistory(string userId)
        {
            List <WithdrawalHistory> history = new List<WithdrawalHistory>(); 
           history =  await _withdrawalHistory.Find(x => x.CustomerId == userId).SortByDescending(x=>x.CreatedAt).ToListAsync();
            if (history != null)
            {
                return history;
            }
            else {
                return null; 
            }
        }

        public async Task UpdateWithdrawalStatus(WithdrawalHistory withdrawalHistory)
        {
            await _withdrawalHistory.ReplaceOneAsync(x => x.Id == withdrawalHistory.Id, withdrawalHistory);
        }
        public async Task<WithdrawalHistory> GetByIdAsync(string id) {

            return  await _withdrawalHistory.Find(x => x.Id == id).FirstOrDefaultAsync(); 
        }
        public async Task<MachineOwnerWithdrawalHistory> GetByMachineOwnerIdAsync(string id) {

            return  await _withdrawalOwnerHistory.Find(x => x.Id == id).FirstOrDefaultAsync(); 
        }

        public async Task<List<MachineOwnerWithdrawalHistory>?> GetWithdrawalHistoryBasedOnOrganizationId(string organizationId)
        {
            var history = await _withdrawalOwnerHistory
                .Find(x => x.OrganizationId == organizationId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();

            return history;
        }

        public async Task<string> AddMachineOwnerWithdrawalHistory(MachineOwnerWithdrawalHistory ownerWithdrawalHistory)
        {
            await _withdrawalOwnerHistory.InsertOneAsync(ownerWithdrawalHistory);
            return ownerWithdrawalHistory.Id;
        }

        public async Task UpdateMachineOwnerWithdrawalStatus(MachineOwnerWithdrawalHistory withdrawalOwnerHistory)
        {
            await _withdrawalOwnerHistory.ReplaceOneAsync(x => x.Id == withdrawalOwnerHistory.Id, withdrawalOwnerHistory);
        }
    }
}
