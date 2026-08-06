using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoRazorPayRepository : IRazorPayRepository
    {
        private readonly IMongoCollection<RazorPayOrder> _collection;

        public MongoRazorPayRepository(IMongoDatabase database) { _collection = database.GetCollection<RazorPayOrder>("RazorPayOrders"); }
        public async Task<RazorPayOrder> CreateOrder(Order order, string userId)
        {
            RazorPayOrder orderPay = new RazorPayOrder();
            orderPay.OrderId = order["id"].ToString();
            orderPay.Amount = order["amount"];
            orderPay.Currency = order["currency"];
            orderPay.Receipt = order["receipt"]; 
            orderPay.CreatedBy = userId;
            await _collection.InsertOneAsync(orderPay);
            return orderPay;
        }

        public async Task<RazorPayOrder> GetOrder(string orderId)
        {
            var order = await _collection
              .Find(u => u.OrderId == orderId)
               .FirstOrDefaultAsync();
            return order;
        }

        public async Task<Result> UpdateOrderSuccess(string orderId, string paymentId)
        {
            var filter = Builders<RazorPayOrder>.Filter.Eq(o => o.OrderId, orderId);

            var update = Builders<RazorPayOrder>.Update
                .Set(o => o.Status, "Success")
                .Set(o => o.PaymentId, paymentId)
                .Set(o => o.ModifiedAt, DateTime.UtcNow);

            var updateResult = await _collection.UpdateOneAsync(filter, update);

            if (updateResult.MatchedCount == 0)
            {
                return Result.Failure($"Order with id '{orderId}' not found.");
            }

            if (updateResult.ModifiedCount == 0)
            {
                return Result.Failure("Order was not updated (already in desired state?).");
            }

            return Result.Success();
        }
    }
}
