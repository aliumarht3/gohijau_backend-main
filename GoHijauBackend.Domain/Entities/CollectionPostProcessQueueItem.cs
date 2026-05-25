using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class CollectionPostProcessQueueItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("MachineId")]
        public string MachineId { get; set; } = string.Empty;

        [BsonElement("AccessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [BsonElement("OilCollected")]
        public double OilCollected { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } = "Pending";

        [BsonElement("Attempts")]
        public int Attempts { get; set; }

        [BsonElement("MaxAttempts")]
        public int MaxAttempts { get; set; } = 3;

        [BsonElement("LastError")]
        public string? LastError { get; set; }

        [BsonElement("NextAttemptAt")]
        public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
