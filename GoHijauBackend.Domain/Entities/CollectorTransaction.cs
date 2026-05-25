using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    [BsonIgnoreExtraElements]
    public class CollectorTransaction
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("UserId")]
        public required string UserId { get; set; }

        [BsonElement("OilCollected")]
        public double OilCollected { get; set; }

        [BsonElement("CollectorRecordedOil")]
        public double CollectorRecordedOil { get; set; }

        [BsonElement("CO2Saved")]
        public double CO2Saved { get; set; }

        [BsonElement("MachineId")]
        public string MachineId { get; set; }

        [BsonElement("AccessToken")]
        public required string AccessToken { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [BsonElement("ModifiedBy")]
        public string ModifiedBy { get; set; }
    }
}
