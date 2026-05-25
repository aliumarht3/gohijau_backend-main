using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class QrToken
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Mongo's `_id`

        [BsonElement("Token")]
        public string Token { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("Status")]
        public string Status { get; set; } = "pending"; // "used", "expired"

        [BsonElement("Panel")]
        public required string Panel { get; set; }

        [BsonElement("MachineId")]
        public string MachineId { get; set; } = string.Empty;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("ExpiresAt")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(3);

        [BsonElement("UsedAt")]
        public DateTime? UsedAt { get; set; }
    }
}
