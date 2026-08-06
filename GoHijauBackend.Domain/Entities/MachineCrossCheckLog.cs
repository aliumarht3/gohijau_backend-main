using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class MachineCrossCheckLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string MachineId { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Load Cell Data
        public double? UltrasonicCm { get; set; }
        public double? ActualWeightKg { get; set; }
        public double? ExpectedWeightKg { get; set; }
        public double? VarianceKg { get; set; }

        // Pump Data
        public double? ReservoirDeltaCm { get; set; }
        public bool? PumpMovedLiquid { get; set; }
        public bool? WeighingTankEmptied { get; set; }
        public double? FinalSmallTankCm { get; set; }
    }
}