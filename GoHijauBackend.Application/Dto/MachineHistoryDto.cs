namespace GoHijauBackend.Application.Dto
{
    public class DepositionHistoryDto
    {
        public string DateTime { get; set; }
        public string Amount { get; set; }
        public string User { get; set; }
        public double CustomerReward { get; set; }
        public double OwnerReward { get; set; }
    }

    public class CollectionHistoryDto
    {
        public string DateTime { get; set; }
        public string Collector { get; set; }
        public string GohijauRecorded { get; set; }
        public string CollectorRecorded { get; set; }
    }
}
