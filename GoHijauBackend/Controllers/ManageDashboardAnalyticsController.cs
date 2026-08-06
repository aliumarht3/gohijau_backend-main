using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/dashboardanalytics")]
    public class ManageDashboardAnalyticsController : ControllerBase
    {
        private readonly IDashboardAnalyticsService _dashboardAnalyticsService ;

        public ManageDashboardAnalyticsController(IDashboardAnalyticsService dashboardAnalyticsService) 
        {
            _dashboardAnalyticsService = dashboardAnalyticsService; 
        }
        [HttpGet("dashboard-info")]
        public async Task<IActionResult> GetDashboardAnalytics()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var analytics = await _dashboardAnalyticsService.GetDashboardAnalytics(userId);
            return Ok(analytics);
        }

    }
}
