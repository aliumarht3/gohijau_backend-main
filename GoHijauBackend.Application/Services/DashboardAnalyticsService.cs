using System;
using System.Threading.Tasks;
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

        // --- NEW SIPOC METHOD ---
        public async Task<SipocAnalyticsDTO> GetSipocAnalytics(string userId, DateTime? startDate, DateTime? endDate)
        {
            // Fetch active machines using your existing repository method
            var totalActiveMachines = await _dashboardAnalyticsRepository.GetOwnerTotalActiveMachines(userId);

            // TODO: Create new methods in your repositories that accept startDate and endDate
            // Example:
            // var totalInput = await _transactionRepository.GetTotalOilPouredByDateRange(userId, startDate, endDate);
            
            var sipocDTO = new SipocAnalyticsDTO
            {
                // Placeholder 0s until the date-filtering repository methods are created
                TotalInputWeight = 0, 
                TotalOutputWeight = 0,
                TotalContamination = 0,
                TotalDowntimeHours = 0,
                ActiveUsers = 0,
                ProcessedTransactions = 0,
                ActiveMachines = totalActiveMachines 
            };

            return sipocDTO;
        }
    }
}