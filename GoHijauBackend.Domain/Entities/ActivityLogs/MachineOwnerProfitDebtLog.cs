using GoHijauBackend.Domain.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Infrastructure.Persistence.ActivityLogs
{
    public class MachineOwnerProfitDebtLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("CustomerTransactionId")]
        public string CustomerTransactionId { get; set; }

        [BsonElement("MachineOwnerOrganizationId")]
        public string MachineOwnerOrganizationId { get; set; }

        [BsonElement("MachineId")]
        public string MachineId { get; set; }

        [BsonElement("CustomerId")]
        public string CustomerId { get; set; }

        [BsonElement("UcoWeight")]
        public double UcoWeight { get; set; }

        [BsonElement("ProfitRate")]
        public double ProfitRate { get; set; }

        [BsonElement("Amount")]
        public decimal Amount { get; set; }

        [BsonElement("Type")]
        [BsonRepresentation(BsonType.String)]
        public MachineOwnerProfitLogType Type { get; set; }

        [BsonElement("NewEwalletBalance")]
        public decimal NewEwalletBalance { get; set; }

        [BsonElement("NewDebtBalance")]
        public decimal NewDebtBalance { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }
    }
}
