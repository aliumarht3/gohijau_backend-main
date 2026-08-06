using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace GoHijauBackend.Domain.Entities
{
    public class ManMachine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("MachineId")]
        public required string MachineId { get; set; }

        [BsonElement("Status")]
        public ManMachineStatus Status { get; set; } = ManMachineStatus.PENDING;
        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [BsonElement("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [SetsRequiredMembers]
        public ManMachine(string machineId, ManMachineStatus status, string userId)
        {
            MachineId = machineId;
            Status = status;
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId;
        }

    }

    public enum ManMachineStatus
    {
       PENDING,
       SENT,
       DELETED
    }
}
