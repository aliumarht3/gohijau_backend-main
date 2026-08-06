namespace GoHijauBackend.Domain.Entities.External
{
    public class PayoutStatus
    {
        public string Reference { get; }
        public string PayoutStatusCode { get; }
        public string RawResponse { get; }
        public bool Success { get; }

        public PayoutStatus(string reference, string payoutStatusCode, bool success, string rawResponse)
        {
            Reference = reference;
            PayoutStatusCode = payoutStatusCode;
            Success = success;
            RawResponse = rawResponse;
        }
    }
}
