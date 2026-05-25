using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;

namespace GoHijauBackend.Application.Services
{
    public class CollectorBillingService(ICollectorBillingRepository collectorBillingRepository, IUserService userService) : ICollectorBillingService
    {
        private readonly ICollectorBillingRepository _collectorBillingRepository = collectorBillingRepository;
        private readonly IUserService _userService = userService;

        private const decimal DefaultCollectorRate = 3.1m;

        public async Task<Result<CollectorBillDto?>> GetBill(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Result.Failure<CollectorBillDto?>("User not found");

            var user = await _userService.GetUserById(userId);
            if (string.IsNullOrEmpty(user?.OrganizationId))
                return Result.Failure<CollectorBillDto?>("Organization not found");

            var orgId = user.OrganizationId;

            var userIds = await _collectorBillingRepository.GetUserIdsByOrganizationAsync(orgId);
            if (userIds == null || userIds.Count == 0)
                return Result.Success<CollectorBillDto?>(new CollectorBillDto());

            var organization = await _collectorBillingRepository.GetOrganizationByIdAsync(orgId);
            var transactions = await _collectorBillingRepository.GetCollectorTransactionsByUserIdsAsync(userIds);
            var orders = await _collectorBillingRepository.GetSuccessfulOrdersByUserIdsAsync(userIds);

            decimal totalOilCollected = (decimal)transactions.Sum(t => t.OilCollected);
            decimal totalOrdersAmount = orders.Sum(o => o.Amount);

            decimal collectorRate = organization?.CollectorRate is { } rate and not 0
                ? (decimal)rate
                : DefaultCollectorRate;

            decimal creditLimit = organization?.CreditLimit is { } cl and not 0
                ? (decimal)cl
                : 0m;

            decimal net = Math.Round(
                (totalOilCollected * collectorRate) - (totalOrdersAmount / 100m),
                2,
                MidpointRounding.AwayFromZero
            );

            var bill = new CollectorBillDto
            {
                currentBill = net,
                overDue = net,
                credit = Math.Max(creditLimit - net, 0m)
            };

            return Result.Success<CollectorBillDto?>(bill);
        }
    }
}
