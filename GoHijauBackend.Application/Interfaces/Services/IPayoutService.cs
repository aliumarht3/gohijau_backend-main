using GoHijauBackend.Domain.Entities.External;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IPayoutService
    {
        Task<PayoutStatus> GetPayoutStatus(string reference);
        Task<PayoutStatus> CreatePayout(PayoutRequest payoutRequest, string userId);
    }
}
