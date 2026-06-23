using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IDashboardAnalyticsService
    {
        Task<DashboardAnalyticsDTO> GetDashboardAnalytics(string userId);
        
        // ADD THIS:
        Task<SipocAnalyticsDTO> GetSipocAnalytics(string userId, DateTime? startDate, DateTime? endDate);
    }
}
