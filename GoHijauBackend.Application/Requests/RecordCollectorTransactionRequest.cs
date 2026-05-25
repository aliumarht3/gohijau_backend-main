namespace GoHijauBackend.Application.Requests
{
    public class RecordCollectorTransactionRequest
    {
        public required string UserId { get; set; }
        public double OilCollected { get; set; }
        public double CO2Saved { get; set; }
        public string MachineId { get; set; }
        public required string AccessToken { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
