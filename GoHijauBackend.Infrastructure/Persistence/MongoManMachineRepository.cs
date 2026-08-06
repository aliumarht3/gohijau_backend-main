using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoManMachineRepository : IManMachineRepository
    {
        private readonly IMongoCollection<ManMachine> _collection;
        public MongoManMachineRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<ManMachine>("ManMachines");
        }

        public async Task AddAsync(ManMachine manMachine)
        {
            await _collection.InsertOneAsync(manMachine);
        }
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string machineId)
        {
            return await _collection.Find(m => m.MachineId == machineId).AnyAsync();
        }

        public async Task<List<ManMachine>> GetAllMachinesAsync()
        {
            return await _collection
               .Find(x => x.Status != ManMachineStatus.DELETED)
               .ToListAsync();
        }

        public async Task<ManMachine?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id && x.Status != ManMachineStatus.DELETED)
                  .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(ManMachine manMachine)
        {
            await _collection.ReplaceOneAsync(x => x.Id == manMachine.Id, manMachine);
        }
    }
}
