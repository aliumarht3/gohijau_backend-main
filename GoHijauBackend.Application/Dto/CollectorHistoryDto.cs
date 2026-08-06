namespace GoHijauBackend.Application.Dto
{
    public class CollectorHistoryDto
    {
        public DateTime DateTime { get; set; }
        public string MachineName { get; set; }
        public string MachineId { get; set; }
        public double SystemRecorded { get; set; }
        public double CollectorRecorded { get; set; }
        public string CollectorName { get; set; }
        public string CollectorOrganization { get; set; }
        public double? CollectorRate { get; set; }
    }
}
