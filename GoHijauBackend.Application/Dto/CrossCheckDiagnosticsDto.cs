namespace GoHijauBackend.Application.Dto
{
    public class CrossCheckDiagnosticsDto
    {
        public string MachineId { get; set; }
        public DateTime Timestamp { get; set; }
        public LoadCellDiagnosticsDto? LoadCellDiagnostics { get; set; }
        public PumpDiagnosticsDto? PumpDiagnostics { get; set; }
    }

    public class LoadCellDiagnosticsDto
    {
        public double UltrasonicCm { get; set; }
        public double ActualWeightKg { get; set; }
        public double ExpectedWeightKg { get; set; }
        public double VarianceKg { get; set; }
    }

    public class PumpDiagnosticsDto
    {
        public double ReservoirDeltaCm { get; set; }
        public bool PumpMovedLiquid { get; set; }
        public bool WeighingTankEmptied { get; set; }
        public double FinalSmallTankCm { get; set; }
    }
}