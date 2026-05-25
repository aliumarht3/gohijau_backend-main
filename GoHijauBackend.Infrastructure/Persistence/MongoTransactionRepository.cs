using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;


namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoTransactionRepository : ITransactionRepository
    {
        private readonly IMongoCollection<Transaction> _collection;
        private readonly IMongoCollection<CollectorTransaction> _collectionCollector;
        private readonly IMongoCollection<Machine> _machine;
        private readonly IMongoCollection<User> _user; 

        public MongoTransactionRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Transaction>("Transactions");
            _collectionCollector = database.GetCollection<CollectorTransaction>("CollectorTransactions");
            _machine= database.GetCollection<Machine>("Machines");
            _user = database.GetCollection<User>("users");

        }

        public async Task<Transaction> InsertTransactionAsync(Transaction transaction, IClientSessionHandle session)
        {
            await _collection.InsertOneAsync(session, transaction);
            return transaction;
        }

        public async Task InsertCollectorTransactionAsync(CollectorTransaction transaction)
        {
            await _collectionCollector.InsertOneAsync(transaction);
        }

        public async Task UpdateCollectorTransactionAsync(CollectorTransaction transaction, IClientSessionHandle session)
        {
            // 1. Find the latest transaction for this user (based on CreatedAt)
            var latestRecord = await _collectionCollector
                .Find(session, x => x.UserId == transaction.UserId)
                .SortByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestRecord == null)
            {
                // If no record exists, create a new one
                transaction.Id = Guid.NewGuid().ToString();
                transaction.CreatedAt = DateTime.UtcNow;

                await _collectionCollector.InsertOneAsync(session, transaction);
                return;
            }

            // 2. Update the latest record found
            var filter = Builders<CollectorTransaction>.Filter.Eq(x => x.Id, latestRecord.Id);

            var update = Builders<CollectorTransaction>.Update
                .Set(x => x.OilCollected, transaction.OilCollected)
                .Set(x => x.CO2Saved, transaction.CO2Saved)
                .Set(x => x.MachineId, transaction.MachineId)
                .Set(x => x.AccessToken, transaction.AccessToken)
                .Set(x => x.ModifiedAt, transaction.ModifiedAt)
                .Set(x => x.ModifiedBy, transaction.ModifiedBy);

            await _collectionCollector.UpdateOneAsync(session, filter, update);
        }

        public async Task<List<Transaction>> GetByUserIdAsync(string user_id)
        {
            return await _collection.Find(trans => trans.UserId == user_id).SortByDescending(trans => trans.CreatedAt).ToListAsync();
        }
        public async Task<double> GetAdminTotalOilPoured()
        {
            var result = await _collection.Aggregate()
         .Group(new BsonDocument
         {
            { "_id", BsonNull.Value },
            { "totalOil", new BsonDocument("$sum", "$OilPoured") }
         })
         .FirstOrDefaultAsync();

            return result?["totalOil"].AsDouble ?? 0;
        }
        public async Task<double> GetOwnerTotalOilPoured(string user_id)
        {
            var user = await _user.Find(m => m.Id == user_id).FirstOrDefaultAsync();
            var machineIds = await _machine
                .Find(m => m.Owner == user.OrganizationId)
                .Project(m => m.MachineId)
                .ToListAsync();

            if (machineIds == null || machineIds.Count == 0)
                return 0;

            // Step 2: Sum up OilPoured from all transactions that match those MachineIds
            var filter = Builders<Transaction>.Filter.In(t => t.MachineId, machineIds);

                    var result = await _collection.Aggregate()
                        .Match(filter)
                        .Group(new BsonDocument
                        {
                    { "_id", BsonNull.Value },
                    { "totalOil", new BsonDocument("$sum", "$OilPoured") }
                })
                .FirstOrDefaultAsync();

            return result?["totalOil"].AsDouble ?? 0;
        }

        public async Task<List<CollectorTransaction>> GetTransactionsByCollectorId(string user_id)
        {
            return await _collectionCollector.Find(trans => trans.UserId == user_id).SortByDescending(trans => trans.CreatedAt).ToListAsync();
        }
        public async Task<List<CollectorTransaction>> GetCollectorTransactionsByUserIdsAsync(List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return new List<CollectorTransaction>();

            var filter = Builders<CollectorTransaction>.Filter.In(ct => ct.UserId, userIds);
            return await _collectionCollector.Find(filter).ToListAsync();
        }
        public async Task<List<Transaction>> GetByMachineIdAsync(string machineId)
        {
            return await _collection.Find(trans => trans.MachineId == machineId).SortByDescending(trans => trans.CreatedAt).ToListAsync();
        }

        public async Task<List<CollectorTransaction>> GetCollectorTransactionsByMachineIdAsync(string machineId)
        {
            return await _collectionCollector.Find(trans => trans.MachineId == machineId).SortByDescending(trans => trans.CreatedAt).ToListAsync();
        }

        public async Task<Transaction?> GetTransactionsByAccessTokenAsync(string accessToken)
        {
            return await _collection
                .Find(Builders<Transaction>.Filter.Eq(t => t.AccessToken, accessToken))
                .FirstOrDefaultAsync();
        }

        public async Task<CollectorTransaction?> GetCollectorTransactionsByAccessTokenAsync(string accessToken)
        {
            return await _collectionCollector
                .Find(Builders<CollectorTransaction>.Filter.Eq(t => t.AccessToken, accessToken))
                .FirstOrDefaultAsync();
        }
    }
}
