using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace GoHijauBackend.Domain.Entities
{
    public class RazorPayOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("OrderId")]
        public string OrderId { set; get; }
        [BsonElement("PaymentId")]
        public string PaymentId { set; get; } = "";
        [BsonElement("Amount")]
        public decimal Amount { set; get; }
        [BsonElement("Status")]
        public string Status { set; get; } = "Pending";
        [BsonElement("Currency")]
        public string Currency { set; get; } = "MYR";
        [BsonElement("Receipt")]
        public string Receipt { set; get; }
        [BsonElement("CreatedBy")]
        public string CreatedBy { set; get; }
        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    }
}
