using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class InvoiceCounter
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("SequenceValue")]
        public int SequenceValue { get; set; }
    }
}
