using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class Transaction
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("UserId")]
        public required string UserId { get; set; }

        [BsonElement("OilPoured")]
        public double OilPoured { get; set; }

        [BsonElement("CO2Saved")]
        public double CO2Saved { get; set; }

        [BsonElement("PointsAwarded")]
        public double PointsAwarded { get; set; }

        [BsonElement("MachineId")]
        public string MachineId { get; set; }

        [BsonElement("AccessToken")]
        public required string AccessToken { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
