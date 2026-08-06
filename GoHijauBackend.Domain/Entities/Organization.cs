using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class Organization
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("OrganizationName")]
        public required string OrganizationName { get; set; }

        [BsonElement("Address")]
        public Address Address { get; set; }

        [BsonElement("OrganizationTypes")]
        public List<OrganizationType> OrganizationTypes { get; set; } = new();

        [BsonElement("CertificatePath")]
        public string CertificatePath { get; set; }

        [BsonElement("CollectorRate")]
        [BsonIgnoreIfNull]
        public double? CollectorRate { get; set; }

        [BsonElement("ProfitRate")]
        [BsonIgnoreIfNull]
        public double? ProfitRate { get; set; }

        [BsonElement("TotalDebtAssigned")]
        [BsonIgnoreIfNull]
        public double? TotalDebtAssigned { get; set; }

        [BsonElement("OutstandingDebt")]
        [BsonIgnoreIfNull]
        public double? OutstandingDebt { get; set; }

        [BsonElement("CustomerRate")]
        [BsonIgnoreIfNull]
        public double? CustomerRate { get; set; }

        [BsonElement("CreditLimit")]
        [BsonIgnoreIfNull]
        public double? CreditLimit { get; set; }

        [BsonElement("InvoiceEmails")]
        [BsonIgnoreIfNull]
        public List<string> InvoiceEmails { get; set; } = new();

        [BsonElement("NotificationEmails")]
        [BsonIgnoreIfNull]
        public List<string> NotificationEmails { get; set; } = new();

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
