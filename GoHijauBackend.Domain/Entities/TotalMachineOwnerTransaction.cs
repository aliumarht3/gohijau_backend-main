using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class TotalMachineOwnerTransaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("OrganizationId")]
        public required string OrganizationId { get; set; }

        [BsonElement("TotalOilCollected")]
        public double TotalOilCollected { get; set; }

        [BsonElement("TotalCO2Saved")]
        public double TotalCO2Saved { get; set; }
        [BsonElement("PointsAwarded")]
        public double PointsAwarded { get; set; }

        [BsonElement("AccessToken")]
        public string AccessToken { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    }
}
