using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    internal class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly IMongoCollection<PasswordResetToken> _collection;

        public PasswordResetRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<PasswordResetToken>("PasswordResetTokens");
        }

        public async Task CreateResetToken(PasswordResetToken token)
        {
            await _collection.InsertOneAsync(token);
        }

        public async Task<PasswordResetToken?> GetResetTokenByTokenId(string tokenId)
        {
            return await _collection
                .Find(t => t.TokenId == tokenId && t.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteResetTokenByTokenId(string tokenId)
        {
            await _collection.DeleteOneAsync(t => t.TokenId == tokenId);
        }
    }
}
