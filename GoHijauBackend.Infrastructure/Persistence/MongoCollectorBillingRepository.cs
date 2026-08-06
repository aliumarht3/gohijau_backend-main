using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoCollectorBillingRepository : ICollectorBillingRepository
    {
        private readonly IMongoCollection<CollectorTransaction> _collectorTransactions;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<RazorPayOrder> _razorPayOrders;
        private readonly IMongoCollection<Organization> _organizations;

        public MongoCollectorBillingRepository(IMongoDatabase database)
        {
            _collectorTransactions = database.GetCollection<CollectorTransaction>("CollectorTransactions");
            _users = database.GetCollection<User>("users");
            _razorPayOrders = database.GetCollection<RazorPayOrder>("RazorPayOrders");
            _organizations = database.GetCollection<Organization>("Organizations");
        }

        public async Task<List<string>> GetUserIdsByOrganizationAsync(string organizationId)
        {
            return await _users
                .Find(u => u.OrganizationId == organizationId && !u.IsDeleted)
                .Project(u => u.Id)
                .ToListAsync();
        }

        public async Task<List<CollectorTransaction>> GetCollectorTransactionsByUserIdsAsync(List<string> userIds)
        {
            return await _collectorTransactions
                .Find(t => userIds.Contains(t.UserId))
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<RazorPayOrder>> GetSuccessfulOrdersByUserIdsAsync(List<string> userIds)
        {
            return await _razorPayOrders
                .Find(o => userIds.Contains(o.CreatedBy) && o.Status == "Success")
                .ToListAsync();
        }

        public async Task<Organization?> GetOrganizationByIdAsync(string organizationId)
        {
            return await _organizations
                .Find(o => o.Id == organizationId)
                .FirstOrDefaultAsync();
        }
    }
}
