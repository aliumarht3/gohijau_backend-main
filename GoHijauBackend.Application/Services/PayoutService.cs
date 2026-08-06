using GoHijauBackend.Application.Interfaces.Persistence.External;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities.External;
using System.Text.Json;

namespace GoHijauBackend.Application.Services
{
    public class PayoutService : IPayoutService
    {
        private readonly ICustomerPayoutGateway _payoutGateway;
        private readonly IWithdrawalHistoryService _withdrawalHistoryService;
        private readonly IBankAccountService _bankAccountService;
        private readonly TransactionService _transactionService;

        public PayoutService(
            ICustomerPayoutGateway payoutGateway, 
            IWithdrawalHistoryService withdrawalHistoryService, 
            IBankAccountService bankAccountService,
            TransactionService transactionService
            )
        {
            _payoutGateway = payoutGateway;
            _withdrawalHistoryService = withdrawalHistoryService;
            _bankAccountService = bankAccountService;
            _transactionService = transactionService;
        }

        public async Task<PayoutStatus> GetPayoutStatus(string reference)
        {
            return await _payoutGateway.GetStatusAsync(reference);
        }

        public async Task<PayoutStatus> CreatePayout(PayoutRequest payoutRequest, string userId)
        {
            bool hasEnoughPoints = await CheckUserRewardEnough(userId, payoutRequest.Amount);
            if (!hasEnoughPoints)
            {
                return new PayoutStatus
                (
                    reference: "123",
                    payoutStatusCode: "400",
                    success: false,
                    rawResponse: "Insufficient reward points for payout."
                );
            }

            var (isSuccess, error, withdrawalHistoryId) = await _withdrawalHistoryService.AddWithdrawalHistory(payoutRequest.Amount.ToString(), userId);
            var bankAccountResponse = await _bankAccountService.GetBankAccount(userId);
            if (bankAccountResponse == null)
            {
                throw new Exception("No bank account found for this user. Try restart the app, should be working.");
            }

            var payoutResponse = await _payoutGateway.CreatePayoutAsync(payoutRequest, withdrawalHistoryId, bankAccountResponse.BankCode, bankAccountResponse.AccountNumber);

            if (payoutResponse.Success)
            {
                var rawResponse = JsonDocument.Parse(payoutResponse.RawResponse);
                var recipientReference = rawResponse.RootElement.GetProperty("Response")[0].GetProperty("recipient_reference")[0].GetString();
                
                await Task.Delay(4000);
                var payoutStatusResponse = await GetPayoutStatus(recipientReference);

                var rawPayoutStatusResponse = JsonDocument.Parse(payoutStatusResponse.RawResponse);
                var payoutStatusCode = rawPayoutStatusResponse.RootElement.GetProperty("Response")[0][0].GetProperty("payout_status_code")[0].GetString();
                
                if (payoutStatusCode == "0")
                {
                    await _transactionService.DeductCustomerTotalPoint(userId, Convert.ToDouble(payoutRequest.Amount));
                    await _withdrawalHistoryService.UpdateWithdrawalStatus(withdrawalHistoryId, "SUCCESS");
                }
                else if (payoutStatusCode == "2" || payoutStatusCode == "7" || payoutStatusCode == "8" || payoutStatusCode == "11")
                {
                    await _withdrawalHistoryService.UpdateWithdrawalStatus(withdrawalHistoryId, "REJECTED");
                }
                else
                {
                    await _transactionService.DeductCustomerTotalPoint(userId, Convert.ToDouble(payoutRequest.Amount));
                    await _withdrawalHistoryService.UpdateWithdrawalStatus(withdrawalHistoryId, "PENDING"); 
                }
            }
            return payoutResponse;
        }
        
        private async Task<bool> CheckUserRewardEnough(string userId, decimal amount)
        {
            var customerPoints = await _transactionService.GetTotalCustomerRewardsByUserId(userId);

            return customerPoints >= amount;
        }
    }
}
