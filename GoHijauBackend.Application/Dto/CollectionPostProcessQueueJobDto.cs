namespace GoHijauBackend.Application.Dto
{
    public class CollectionPostProcessQueueJobDto
    {
        public string JobId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public double OilCollected { get; set; }
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
        public string? LastError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
