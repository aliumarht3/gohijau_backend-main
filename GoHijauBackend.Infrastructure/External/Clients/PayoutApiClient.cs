using GoHijauBackend.Infrastructure.External.Clients;
using Microsoft.Extensions.Logging;

namespace GoHijauBackend.Domain.Entities.External.Clients
{
    public class PayoutApiClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<PayoutApiClient> _logger;
        private string? _sessionCookie;

        private const string LOGIN_URL = "https://curlecuat.payouts.curlec.com/login";
        private const string CREATE_URL = "https://curlecuat.payouts.curlec.com/curlec-services/payouts/quick";
        private const string STATUS_URL = "https://curlecuat.payouts.curlec.com/curlec-services/payouts/status";

        public PayoutApiClient(IHttpClientFactory clientFactory, ILogger<PayoutApiClient> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task EnsureLoginAsync()
        {
            if (!string.IsNullOrEmpty(_sessionCookie))
                return; // reuse existing cookie

            var client = _clientFactory.CreateClient();
            var loginForm = new MultipartFormDataContent
            {
                { new StringContent("Atia_Admin"), "username" },
                { new StringContent("Curlec@123"), "password" }
            };

            var response = await client.PostAsync(LOGIN_URL, loginForm);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Login failed: {Body}", body);
                throw new Exception("Login failed: " + body);
            }

            if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
            {
                _sessionCookie = string.Join("; ", cookieValues);
            }

            if (string.IsNullOrEmpty(_sessionCookie))
                throw new Exception("No cookie returned from login");
        }

        public async Task<ApiResponse> PostAsync(string url, Dictionary<string, string> formData)
        {
            await EnsureLoginAsync();

            var client = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(formData)
            };
            request.Headers.Add("Cookie", _sessionCookie);

            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("API call failed: {Body}", body);

            return new ApiResponse
            {
                Body = body,
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode
            };
        }

        public string GetCreateUrl() => CREATE_URL;
        public string GetStatusUrl() => STATUS_URL;
    }
}
