using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace GoHijauBackend.Domain.Entities
{
    public class MachineAudit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public required string UserId { get; set; }

        [BsonElement("Action")]
        public required string Action { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [SetsRequiredMembers]
        public MachineAudit(string machineId, string userId, string action)
        {
            UserId = userId;
            Action = action;
            CreatedAt = DateTime.UtcNow;
            CreatedBy = machineId;
        }
    }
}
