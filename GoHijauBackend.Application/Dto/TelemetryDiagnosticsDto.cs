namespace GoHijauBackend.Application.Dto
{
    public class IncomingPythonTelemetry
    {
        public string MachineId { get; set; }
        public MetricsData Metrics { get; set; }
    }

    public class MachineTelemetry
    {
        public string MachineId { get; set; }
        public bool IsOnline { get; set; }
        public MetricsData Metrics { get; set; }
    }

    public class MetricsData
    {
        public double WeightKg { get; set; }
        public double MainTankVolumeLiters { get; set; }
        public double TurbidityValue { get; set; }
        public double JunkTankDistanceCm { get; set; }
    }

    public class DiagnosticLogDto
    {
        public string MachineId { get; set; }
        public double Timestamp { get; set; }
        public int No { get; set; }
        public string Type { get; set; } // "Online" or "Physical"
        public string Component { get; set; }
        public string Checking { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
    }
}