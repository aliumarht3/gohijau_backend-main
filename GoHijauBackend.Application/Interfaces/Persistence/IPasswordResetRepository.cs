using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IPasswordResetRepository
    {
        Task CreateResetToken(PasswordResetToken token);
        Task<PasswordResetToken?> GetResetTokenByTokenId(string tokenId);
        Task DeleteResetTokenByTokenId(string tokenId);
    }
}
