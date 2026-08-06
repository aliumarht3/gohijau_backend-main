using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;


namespace GoHijauBackend.Application.Services
{
    public class RateService(IRateRepository rateRepository) : IRateService
    {
        private readonly IRateRepository _rateRepository = rateRepository;

        public async Task<Result> AddRate(RateDTO rateDTO)
        {
            var rateModel = new Rate
            {
                UserId = rateDTO.UserId,
                CustomerSellingRate = rateDTO.CustomerSellingRate,
                CollectorBuyingRate = rateDTO.CollectorBuyingRate,

            };
             await _rateRepository.AddAsync(rateModel);
            return Result.Success(); 
        }

        public async Task<RateDTO?> GetLatestRate()
        {
            return await _rateRepository.GetLatest();
        }
    }
}
