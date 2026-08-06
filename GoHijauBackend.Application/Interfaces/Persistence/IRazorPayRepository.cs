using CSharpFunctionalExtensions;
using GoHijauBackend.Domain.Entities;
using Razorpay.Api;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IRazorPayRepository
    {
        public Task<RazorPayOrder> CreateOrder(Order order, string userId); 
        public Task<RazorPayOrder> GetOrder(string orderId); 
        public Task<Result> UpdateOrderSuccess(string orderId, string paymentId); 
    }
}
