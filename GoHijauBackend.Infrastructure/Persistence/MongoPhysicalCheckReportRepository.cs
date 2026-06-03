using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoPhysicalCheckReportRepository : IPhysicalCheckReportRepository
    {
        private readonly IMongoCollection<PhysicalCheckReport> _collection;

        public MongoPhysicalCheckReportRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDB:Database"]);
            _collection = database.GetCollection<PhysicalCheckReport>("PhysicalCheckReports");
        }

        public async Task CreateReportAsync(PhysicalCheckReport report)
        {
            await _collection.InsertOneAsync(report);
        }

        public async Task<List<PhysicalCheckReport>> GetReportsByMachineIdAsync(string machineId)
        {
            return await _collection.Find(r => r.MachineId == machineId)
                                    .SortByDescending(r => r.Timestamp)
                                    .ToListAsync();
        }
    }
}