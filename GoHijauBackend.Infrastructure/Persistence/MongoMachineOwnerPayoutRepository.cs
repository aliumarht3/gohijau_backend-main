using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;


namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoMachineOwnerPayoutRepository : IMachineOwnerPayoutRepository
    {
        private readonly IMongoCollection<Machine> _collection;
    }
}
