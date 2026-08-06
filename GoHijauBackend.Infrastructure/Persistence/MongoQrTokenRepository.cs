using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoQrTokenRepository : IQrTokenRepository
    {
        private readonly IMongoCollection<QrToken> _collection;

        public MongoQrTokenRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<QrToken>("qr_tokens");
        }

        public async Task<QrToken> CreateAsync(QrToken token)
        {
            await _collection.InsertOneAsync(token);
            return token;
        }

        public async Task<QrToken?> GetByTokenAsync(string token)
        {
            return await _collection.Find(t => t.Token == token).FirstOrDefaultAsync();
        }

        public async Task MarkUsedAsync(string token, string machineId)
        {
            var update = Builders<QrToken>.Update
                .Set(t => t.Status, "used")
                .Set(t => t.UsedAt, DateTime.UtcNow)
                .Set(t => t.MachineId, machineId);
            await _collection.UpdateOneAsync(t => t.Token == token, update);
        }
    }

}
