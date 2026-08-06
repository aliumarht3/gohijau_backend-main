using GoHijauBackend.Application.Interfaces.Persistence.External;
using GoHijauBackend.Domain.Entities.External;
using GoHijauBackend.Domain.Entities.External.Clients;
using System.Text.Json;

namespace GoHijauBackend.Infrastructure.External
{
    public class CustomerPayoutGateway : ICustomerPayoutGateway
    {
        private readonly PayoutApiClient _apiClient;

        public CustomerPayoutGateway(PayoutApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PayoutStatus> GetStatusAsync(string recipientReference)
        {
            var data = new Dictionary<string, string>
            {
                { "model", "QPM" },
                { "recipientReference", recipientReference }
            };

            var response = await _apiClient.PostAsync(_apiClient.GetStatusUrl(), data);

            var doc = JsonDocument.Parse(response.Body);

            var statusCode = doc.RootElement.GetProperty("Status")[0].GetString();
            return new PayoutStatus(recipientReference, statusCode, response.Success, response.Body);
        }

        public async Task<PayoutStatus> CreatePayoutAsync(
            PayoutRequest payoutRequest, 
            string withdrawalHistoryId, 
            string bankCode, 
            string accountNumber,
            string details = "CustomerPayout"
        )
        {
            var payoutContent = new Dictionary<string, string>
            {
                { "merchantId", "209383472" },
                { "employeeId", "21106882" },
                { "amount", payoutRequest.Amount.ToString() },
                { "recipientRef", withdrawalHistoryId },
                { "accountNumber", accountNumber },
                { "bankCode", bankCode },
                { "details", "CustomerPayout" }
            };

            var response = await _apiClient.PostAsync(_apiClient.GetCreateUrl(), payoutContent);

            var doc = JsonDocument.Parse(response.Body);
            var statusCode = doc.RootElement.GetProperty("Status")[0].GetString();

            //Later Log Anything happens here
            if (statusCode == "200" || statusCode == "201")
            {
                return new PayoutStatus(null, statusCode, response.Success, response.Body);
            }
            else
            {
                return new PayoutStatus(null, statusCode, false, response.Body);
            }
        }
    }
}
