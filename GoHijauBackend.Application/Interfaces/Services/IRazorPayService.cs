using CSharpFunctionalExtensions;
using GoHijauBackend.Domain.Entities;
using Razorpay.Api;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IRazorPayService
    {
      public Task<Order> CreateOrder(decimal amount, string userId); 
      public Task<RazorPayOrder> GetOrder(string orderId);
      public Task<Result> MarkPaymentAsPaid(string orderId, string paymentId); 
    }
}
