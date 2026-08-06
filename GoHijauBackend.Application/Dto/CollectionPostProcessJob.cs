namespace GoHijauBackend.Application.Dto
{
    public class CollectionPostProcessJob
    {
        public string JobId { get; set; } = string.Empty;
        public required string UserId { get; set; }
        public required string MachineId { get; set; }
        public required string AccessToken { get; set; }
        public double OilCollected { get; set; }
    }
}
