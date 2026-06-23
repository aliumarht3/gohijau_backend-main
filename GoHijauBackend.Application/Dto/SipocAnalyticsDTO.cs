namespace GoHijauBackend.Application.Dto
{
    public class SipocAnalyticsDTO
    {
        public double TotalInputWeight { get; set; } = 0;
        public double TotalOutputWeight { get; set; } = 0;
        public double TotalContamination { get; set; } = 0;
        public double TotalDowntimeHours { get; set; } = 0;
        public int ActiveUsers { get; set; } = 0;
        public int ProcessedTransactions { get; set; } = 0;
        public int ActiveMachines { get; set; } = 0;
    }
}