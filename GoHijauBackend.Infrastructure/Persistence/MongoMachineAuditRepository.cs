using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoMachineAuditRepository : IMachineAuditRepository
    {
        private readonly IMongoCollection<MachineAudit> _collection;

        public MongoMachineAuditRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<MachineAudit>("MachineAudit");
        }

        public async Task AddAudit(MachineAudit machineAudit) =>
            await _collection.InsertOneAsync(machineAudit);
    }
}
