using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class RefreshToken
    {
        [BsonId]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("Token")]
        public string Token { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
