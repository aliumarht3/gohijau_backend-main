using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class Role
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("RoleId")]
        public int RoleId { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }
    }

    public enum UserRole
    {
        Customer = 0,
        Admin = 1,
        Owner = 2,
        GoHijauOwner = 3,
        Technician = 4,
        OilCollector = 5,
        Manufacturer = 6
    }
}
