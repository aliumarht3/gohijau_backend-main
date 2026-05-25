using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    internal class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IMongoCollection<RefreshToken> _refreshToken;

        public RefreshTokenRepository(IMongoDatabase database)
        {
            _refreshToken = database.GetCollection<RefreshToken>("refresh_tokens");
        }

        public async Task SaveAsync(RefreshToken token)
        {
            await _refreshToken.InsertOneAsync(token);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _refreshToken
                .Find(rt => rt.Token == token && rt.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(string token)
        {
            await _refreshToken.DeleteOneAsync(rt => rt.Token == token);
        }
    }
}
