using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using GoHijauBackend.Application.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    
    public class MongoRateRepository : IRateRepository
    {
        private readonly IMongoCollection<Rate> _collection;
        public MongoRateRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Rate>("Rate");
        }
        public async Task AddAsync(Rate rate)
        {
           await _collection.InsertOneAsync(rate);
        }

        public async Task<RateDTO?> GetLatest()
        {
            var latestRate = await _collection
             .Find(_ => true)
             .SortByDescending(r => r.CreatedAt)
             .FirstOrDefaultAsync();

            if (latestRate == null)
                return null;

            var dto = new RateDTO
            {
                Id = latestRate.Id,
                UserId = latestRate.UserId,
                CustomerSellingRate = latestRate.CustomerSellingRate,
                CollectorBuyingRate = latestRate.CollectorBuyingRate,
                CreatedAt = latestRate.CreatedAt,
            };

            return dto;
        }
    }
}
