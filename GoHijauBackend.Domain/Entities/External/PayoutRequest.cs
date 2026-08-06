namespace GoHijauBackend.Domain.Entities.External
{
    public class PayoutRequest
    {
        public decimal Amount { get; }

        public PayoutRequest(decimal amount)
        {
            Amount = amount;
        }
    }
}
