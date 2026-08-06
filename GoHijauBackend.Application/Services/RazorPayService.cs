using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Razorpay.Api;


namespace GoHijauBackend.Application.Services
{
    public class RazorPayService : IRazorPayService
    {
        private readonly IRazorPayRepository _razorPayRepository;
        private readonly ISecretRepository _secretRepository;
        public RazorPayService(IRazorPayRepository razorPayRepository, ISecretRepository secretRepository)
        {
            _razorPayRepository = razorPayRepository;
            _secretRepository = secretRepository;
        }

        public async Task<Order> CreateOrder(decimal amount, string uId)
        {
            try 
            {
                var secret = await _secretRepository.GetSecret();
                if (secret != null) 
                {
                    RazorpayClient client = new RazorpayClient(secret.KeyId, secret.KeySecret);
                    Dictionary<string, object> options = new Dictionary<string, object>();
                    string receiptId = uId + DateTime.UtcNow.ToString("dd/MM/yyyy"); 
                    options.Add("amount", amount);
                    options.Add("receipt", receiptId);
                    options.Add("currency", "MYR");
                    Order order = new Order();
                    order = client.Order.Create(options);
                    var result = _razorPayRepository.CreateOrder(order, uId);
                    return await Task.FromResult(order);
                }
                return new Order();

            } catch (Exception ex) 
            {
                return new Order();
            }
           
        }

        public Task<RazorPayOrder> GetOrder(string orderId)
        {
           return _razorPayRepository.GetOrder(orderId);
        }

        public async Task<Result> MarkPaymentAsPaid(string orderId, string paymentId)
        {
            try 
            {
                var result = await _razorPayRepository.UpdateOrderSuccess(orderId,paymentId); 
                return Result.Success();
            }catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }
    }
}
