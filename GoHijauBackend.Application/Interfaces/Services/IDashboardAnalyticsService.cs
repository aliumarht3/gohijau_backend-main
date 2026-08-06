using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IDashboardAnalyticsService
    {
        public Task<DashboardAnalyticsDTO> GetDashboardAnalytics(string userId);
    }
}
