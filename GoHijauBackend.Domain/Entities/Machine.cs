using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace GoHijauBackend.Domain.Entities
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("MachineId")]
        public required string MachineId { get; set; }

        [BsonElement("Location")]
        public Location Location { get; set; }

        [BsonElement("Type")]
        public MachineType Type { get; set; }

        [BsonElement("ManufacturedDate")]
        public DateTime ManufacturedDate { get; set; }

        [BsonElement("Status")]
        public MachineStatus Status { get; set; } = MachineStatus.DEPLOYED;

        [BsonElement("Owner")]
        public string Owner { get; set; }

        [BsonElement("Collector")]
        public string Collector { get; set; }

        [BsonElement("Technician")]
        public string Technician { get; set; }
        [BsonElement("Sent80PercentReminder")]
        public bool Sent80PercentReminder { get; set; } = false;
        [BsonElement("Sent100PercentReminder")]
        public bool Sent100PercentReminder { get; set; } = false ;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [BsonElement("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [SetsRequiredMembers]
        public Machine(string machineId, Location location, MachineType type, DateTime manufacturedDate, MachineStatus status, string owner, string collector, string technician, string userId)
        {
            MachineId = machineId;
            Location = location;
            Type = type;
            ManufacturedDate = manufacturedDate;
            Status = status;
            Owner = owner;
            Collector = collector;
            Technician = technician;
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
        }
    }

    public enum MachineStatus
    {
        DEPLOYED,
        RUNNING,
        STOPPED,
        MAINTENANCE,
        DELETED
    }

    public enum MachineType
    {
        UCO_COLLECTOR
    }
}
