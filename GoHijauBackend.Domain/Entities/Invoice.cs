using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class Invoice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("InvoiceId")]
        public string InvoiceId { get; set; } = string.Empty;
        [BsonElement("OrganizationId")]
        public string OrganizationId { get; set; } = string.Empty;
        [BsonElement("MachineId")]
        public string MachineId { get; set; } = string.Empty;
        [BsonElement("OilCollected")]
        public double OilCollected { get; set; }
        [BsonElement("CollectorRate")]
        public double CollectorRate { get; set; }
        [BsonElement("TotalAmount")]
        public double TotalAmount { get; set; }
        [BsonElement("PdfPath")]
        public string PdfPath { get; set; } = string.Empty;
        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; } = string.Empty;
    }
}
