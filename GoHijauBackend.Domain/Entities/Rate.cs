using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class Rate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public required string UserId { get; set; }

        [BsonElement("CustomerSellingRate")]
        public float CustomerSellingRate { get; set; }

        [BsonElement("CollectorBuyingRate")]
        public float CollectorBuyingRate { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
