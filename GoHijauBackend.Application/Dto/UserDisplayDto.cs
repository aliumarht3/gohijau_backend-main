using GoHijauBackend.Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Application.Dto
{
    [BsonIgnoreExtraElements]
    public class UserJoined : User
    {
        public List<Organization> OrganizationData { get; set; } = new();
        public double PointsAwarded { get; set; } = new();
    }
    public class UserDisplayDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string NricOrPassport { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public double PointsAwarded { get; set; }
        public HashSet<int> RolesId { get; set; }
        public List<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
