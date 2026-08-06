using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Domain.Entities
{
    public class Reminder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("OrganizationId")]
        public string OrganizationId { get; set; } = string.Empty;
        [BsonElement("MachineId")]
        public string MachineId { get; set; } = string.Empty ;
        [BsonElement("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        [BsonElement("Message")]
        public string Message { get; set; } = string.Empty;
        [BsonElement("ReminderTimeUtc")]
        public DateTime ReminderTimeUtc { get; set; } = DateTime.UtcNow;
        [BsonElement("Status")]
        public string Status { get; set; } = "Pending";
        [BsonElement("ErrorMessage")]
        public string? ErrorMessage { get; set; }
        [BsonElement("CreatedAtUtc")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
