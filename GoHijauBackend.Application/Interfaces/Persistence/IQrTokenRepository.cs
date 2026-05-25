using GoHijauBackend.Domain.Entities;



namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IQrTokenRepository
    {
        Task<QrToken> CreateAsync(QrToken token);
        Task<QrToken?> GetByTokenAsync(string token);
        Task MarkUsedAsync(string token, string machineId);
    }
}
