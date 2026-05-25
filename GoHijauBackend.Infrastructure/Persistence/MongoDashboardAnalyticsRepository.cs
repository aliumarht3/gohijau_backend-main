using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoDashboardAnalyticsRepository : IDashboardAnalyticsRepository
    {
        private readonly IMongoCollection<Machine> _machine;
        private readonly IMongoCollection<User> _user;
        private readonly IMongoCollection<Organization> _organization;
        public MongoDashboardAnalyticsRepository(IMongoDatabase database) {
            _machine = database.GetCollection<Machine>("Machines");
            _user = database.GetCollection<User>("users");
            _organization = database.GetCollection<Organization>("Organizations");
        }

        public async Task<int> GetOwnerTotalActiveMachines(string userId)
        {
            var user = await _user.Find(m => m.Id == userId).FirstOrDefaultAsync();
            var machineIds = await _machine
                .Find(m => m.Owner == user.OrganizationId)
                .Project(m => m.MachineId)
                .ToListAsync();

            if (machineIds == null || machineIds.Count == 0)
                return 0;
            return machineIds.Count;
        }

        public async Task<double> GetOwnerTotalIncome(string userId, double totalUCO)
        {
            var user = await _user.Find(m => m.Id == userId).FirstOrDefaultAsync();

            var organization = await _organization.Find(m => m.Id == user.OrganizationId).FirstOrDefaultAsync();
            if (user == null || organization == null)
                return 0;

        

            var total = organization.CollectorRate != null ? organization.CollectorRate * totalUCO : 0.9*totalUCO;

            return Math.Round((double)total, 2);


        }
    }
}
