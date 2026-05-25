using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class TotalTransaction
    {
        [BsonId]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public required string UserId { get; set; }

        [BsonElement("TotalOilPoured")]
        public double TotalOilPoured { get; set; }

        [BsonElement("TotalCO2Saved")]
        public double TotalCO2Saved { get; set; }

        [BsonElement("PointsAwarded")]
        public double PointsAwarded { get; set; }

        [BsonElement("AccessToken")]
        public required string AccessToken { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; }
    }
}
