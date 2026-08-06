using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;


namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoTotalTransactionRepository : ITotalTransactionRepository
    {
        private readonly IMongoCollection<TotalTransaction> _collection;
        private readonly IMongoCollection<TotalCollectorTransaction> _collectionCollector;
        private readonly IMongoCollection<TotalMachineOwnerTransaction> _machineOwnerCollection;
        public MongoTotalTransactionRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<TotalTransaction>("TotalTransactions");
            _collectionCollector = database.GetCollection<TotalCollectorTransaction>("TotalCollectorTransactions");
            _machineOwnerCollection = database.GetCollection<TotalMachineOwnerTransaction>("TotalMachineOwnerTransactions"); 
        }
        public async Task UpdateTotalsAsync(string userId, double oilPoured, double co2Saved, double points, string accessToken, IClientSessionHandle session)
        {
            var update = Builders<TotalTransaction>.Update
                .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow)
               .Inc(x => x.TotalOilPoured, oilPoured)
               .Inc(x => x.TotalCO2Saved, co2Saved)
               .Inc(x => x.PointsAwarded, points)
               .Set(x => x.AccessToken, accessToken)
               .Set(x => x.ModifiedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(
                session,
                Builders<TotalTransaction>.Filter.Eq(x => x.UserId, userId),
                update,
                new UpdateOptions { IsUpsert = true }
            );
        }

        public async Task<TotalTransaction?> GetByUserIdAsync(string userId)
        {
            var result = await _collection.Find(trans => trans.UserId == userId).FirstOrDefaultAsync();

            if (result != null)
            {
                result.TotalOilPoured = Math.Round(result.TotalOilPoured, 2);
                result.TotalCO2Saved = Math.Round(result.TotalCO2Saved, 2);
                result.PointsAwarded = Math.Round(result.PointsAwarded, 2);
            }

            return result;
        }

        public async Task UpdateCollectorTotalsAsync(string userId, double oilCollected, double co2Saved, string accessToken, IClientSessionHandle session)
        {
            var update = Builders<TotalCollectorTransaction>.Update
                         .SetOnInsert(x => x.Id, Guid.NewGuid().ToString())
                         .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow)
                        .Inc(x => x.TotalOilCollected, oilCollected)
                        .Inc(x => x.TotalCO2Saved, co2Saved)
                        .Set(x => x.AccessToken, accessToken)
                        .Set(x => x.ModifiedAt, DateTime.UtcNow);

            await _collectionCollector.UpdateOneAsync(
                session,
                Builders<TotalCollectorTransaction>.Filter.Eq(x => x.UserId, userId),
                update,
                new UpdateOptions { IsUpsert = true }
            );
        }

        public async Task<TotalMachineOwnerTransaction?> UpdateMachineOwnerTotalsAsync(string organizationId, double oilCollected, double co2Saved, double points, string accessToken, IClientSessionHandle session)
        {
            var update = Builders<TotalMachineOwnerTransaction>.Update
                         .SetOnInsert(x => x.Id, ObjectId.GenerateNewId().ToString())
                         .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow)
                        .Inc(x => x.TotalOilCollected, oilCollected)
                        .Inc(x => x.TotalCO2Saved, co2Saved)
                        .Inc(x => x.PointsAwarded, points)
                        .Set(x => x.AccessToken, accessToken)
                        .Set(x => x.ModifiedAt, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<TotalMachineOwnerTransaction>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            return await _machineOwnerCollection.FindOneAndUpdateAsync(
                session,
                Builders<TotalMachineOwnerTransaction>.Filter.Eq(x => x.OrganizationId, organizationId),
                update,
                options
            );
        }

        public async Task<decimal> GetCustomerTotalPointsAwarded(string userId)
        {
            var result = await _collection.Aggregate()
                .Match(new BsonDocument("UserId", userId))
                .Group(new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalPoints", new BsonDocument("$sum", "$PointsAwarded") }
                })
                .FirstOrDefaultAsync();

            return result?["totalPoints"].ToDecimal() ?? 0;
        }
        public async Task<decimal> GetMachineOwnerTotalPointsAwarded(string organizationId)
        {
            var result = await _machineOwnerCollection.Aggregate()
                .Match(new BsonDocument("OrganizationId", organizationId))
                .Group(new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "totalPoints", new BsonDocument("$sum", "$PointsAwarded") }
                })
                .FirstOrDefaultAsync();

            return result?["totalPoints"].ToDecimal() ?? 0;
        }

        public async Task<decimal> GetAllMachineOwnersTotalPointsAwarded()
        {
            var result = await _machineOwnerCollection.Aggregate()
                .Group(new BsonDocument
                {
            { "_id", BsonNull.Value },
            {
                "totalPoints",
                new BsonDocument("$sum",
                    new BsonDocument("$ifNull", new BsonArray { "$PointsAwarded", 0 })
                )
            }
                })
                .FirstOrDefaultAsync();

            return result != null && result.Contains("totalPoints")
                ? result["totalPoints"].ToDecimal()
                : 0m;
        }

        public async Task<bool> DeductCustomerTotalPoint(string userId, double points)
        {
            var update = Builders<TotalTransaction>.Update
               .Inc(x => x.PointsAwarded, -points)
               .Set(x => x.ModifiedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(
                Builders<TotalTransaction>.Filter.Eq(x => x.UserId, userId),
                update,
                new UpdateOptions { IsUpsert = false }
            );

            return result.ModifiedCount > 0;
        }
        public async Task<bool> DeductMachineOwnerTotalPoint(string organizationId, double points)
        {
            var update = Builders<TotalMachineOwnerTransaction>.Update
               .Inc(x => x.PointsAwarded, -points)
               .Set(x => x.ModifiedAt, DateTime.UtcNow);

            var result = await _machineOwnerCollection.UpdateOneAsync(
                Builders<TotalMachineOwnerTransaction>.Filter.Eq(x => x.OrganizationId, organizationId),
                update,
                new UpdateOptions { IsUpsert = false }
            );

            return result.ModifiedCount > 0;
        }

        public async Task<TotalMachineOwnerTransaction?> GetByOrganizationIdAsync(string organizationId)
        {
            if (string.IsNullOrWhiteSpace(organizationId))
                return null;

            return await _machineOwnerCollection
                .Find(x => x.OrganizationId == organizationId)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetAllCustomersTotalPointsAwarded()
        {
            var result = await _collection.Aggregate()
                .Group(new BsonDocument
                {
            { "_id", BsonNull.Value },
            {
                "totalPoints",
                new BsonDocument("$sum",
                    new BsonDocument("$ifNull", new BsonArray { "$PointsAwarded", 0 })
                )
            }
                })
                .FirstOrDefaultAsync();

            return result?["totalPoints"].ToDecimal() ?? 0;
        }
    }
}
