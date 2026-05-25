using GoHijauBackend.Application.Interfaces.Persistence.ActivityLogs;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence.ActivityLogs
{
    public class MachineOwnerProfitDebtLogRepository : IMachineOwnerProfitDebtLogRepository
    {
        private readonly IMongoCollection<MachineOwnerProfitDebtLog> _collection;
        private const string CollectionName = "LogsMachineOwnerProfitDebt";
        public MachineOwnerProfitDebtLogRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<MachineOwnerProfitDebtLog>(CollectionName);
            EnsureCollectionExists(database).GetAwaiter().GetResult();
        }

        public async Task InsertLog(MachineOwnerProfitDebtLog log, IClientSessionHandle session)
        {
            await _collection.InsertOneAsync(session, log);
        }

        private async Task EnsureCollectionExists(IMongoDatabase database)
        {
            var collectionNames = await database.ListCollectionNamesAsync();
            var collections = await collectionNames.ToListAsync();

            if (!collections.Contains(CollectionName))
            {
                await database.CreateCollectionAsync(CollectionName);
            }
        }
    }
}
