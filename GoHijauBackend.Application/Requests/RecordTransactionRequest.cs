namespace GoHijauBackend.Application.Requests
{
    public class RecordTransactionRequest
    {
        public required string UserId { get; set; }
        public double OilPoured { get; set; }
        public double CO2Saved { get; set; }
        public double PointsAwarded { get; set; }
        public string MachineId { get; set; }
        public required string AccessToken { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
