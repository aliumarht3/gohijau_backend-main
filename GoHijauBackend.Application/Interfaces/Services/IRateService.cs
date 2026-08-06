using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;


namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IRateService
    {
        Task<Result> AddRate(RateDTO rateDTO);
        Task<RateDTO?> GetLatestRate();
    }
}
