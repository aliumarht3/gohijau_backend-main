using GoHijauBackend.Domain.Entities.External;

namespace GoHijauBackend.Application.Interfaces.Persistence.External
{
    public interface ICustomerPayoutGateway
    {
        Task<PayoutStatus> GetStatusAsync(string recipientReference);
        Task<PayoutStatus> CreatePayoutAsync(PayoutRequest payoutRequest, string withdrawalHistoryId, string bankCode, string accountNumber, string details = "CustomerPayout");
    }
}
