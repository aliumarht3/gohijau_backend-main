using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Helpers;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;

namespace GoHijauBackend.Application.Services
{
    public class DashboardAnalyticsService : IDashboardAnalyticsService
    {
        private readonly IDashboardAnalyticsRepository _dashboardAnalyticsRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITotalTransactionRepository _totalTransactionRepository;
        

        public DashboardAnalyticsService(IDashboardAnalyticsRepository dashboardAnalyticsRepository, ITransactionRepository transactionRepository, ITotalTransactionRepository totalTransactionRepository) 
        {
            _dashboardAnalyticsRepository = dashboardAnalyticsRepository;
            _transactionRepository = transactionRepository;
            _totalTransactionRepository = totalTransactionRepository;
        }

        public async Task<DashboardAnalyticsDTO> GetDashboardAnalytics(string userId)
        {
            var TotalUCOPoured = await _transactionRepository.GetOwnerTotalOilPoured(userId);
            var TotalIncome = await _dashboardAnalyticsRepository.GetOwnerTotalIncome(userId,TotalUCOPoured); 
            var TotalActiveMachines = await _dashboardAnalyticsRepository.GetOwnerTotalActiveMachines(userId);
            var TotalCustomersPointsAwarded = await _totalTransactionRepository.GetAllCustomersTotalPointsAwarded();
            var TotalAllMachineOwnersEwalletBalance = await _totalTransactionRepository.GetAllMachineOwnersTotalPointsAwarded();
            DashboardAnalyticsDTO dashboardAnalyticsDTO = new DashboardAnalyticsDTO();
            dashboardAnalyticsDTO.TotalUCO = TotalUCOPoured;
            dashboardAnalyticsDTO.TotalRevenue = TotalIncome;
            dashboardAnalyticsDTO.ActiveMachines = TotalActiveMachines;
            dashboardAnalyticsDTO.TotalCustomersPointsAwarded = MoneyHelper.RoundMoney(TotalCustomersPointsAwarded).ToString("F2");
            dashboardAnalyticsDTO.TotalAllMachineOwnersEwalletBalance = MoneyHelper.RoundMoney(TotalAllMachineOwnersEwalletBalance).ToString("F2");

            return dashboardAnalyticsDTO;

        }
    }
}
