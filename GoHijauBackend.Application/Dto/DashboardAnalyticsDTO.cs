namespace GoHijauBackend.Application.Dto
{
    public class DashboardAnalyticsDTO
    {
        public double TotalRevenue { get; set; } = 0; 
        public int ActiveMachines { get; set; } = 0; 
        public double TotalUCO { get; set; } = 0;
        public string TotalCustomersPointsAwarded { get; set; } = string.Empty;
        public string TotalAllMachineOwnersEwalletBalance { get; set; } = string.Empty;

    }
}
