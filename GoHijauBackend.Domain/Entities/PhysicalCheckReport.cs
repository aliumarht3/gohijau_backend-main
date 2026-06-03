using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class PhysicalCheckReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string MachineId { get; set; }
        public DateTime Timestamp { get; set; }
        public List<PhysicalCheckItem> Checks { get; set; } = new();
    }

    public class PhysicalCheckItem
    {
        public string Component { get; set; }
        public bool Passed { get; set; }
    }
}