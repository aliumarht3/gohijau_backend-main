using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GoHijauBackend.Domain.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("NricOrPassport")]
        public string NricOrPassport { get; set; }

        [BsonElement("OrganizationId")]
        public string OrganizationId { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("CreatedBy")]
        public string CreatedBy { get; set; }

        [BsonElement("ModifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [BsonElement("ModifiedBy")]
        public string ModifiedBy { get; set; }

        [BsonElement("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [BsonElement("DeletedAt")]
        [BsonIgnoreIfNull]
        public DateTime? DeletedAt { get; set; }

        [BsonElement("DeletedBy")]
        [BsonIgnoreIfNull]
        public string? DeletedBy { get; set; }

        [BsonElement("Roles")]
        public HashSet<int> Roles { get; set; } = new();

        public User()
        {
            // Needed for MongoDB deserialization
        }

        public User(string email, string name, string passwordHash, string phone, string nricOrPassport, string organizationId, string userId, IEnumerable<int>? roles = null)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Phone = phone;
            NricOrPassport = nricOrPassport;
            OrganizationId = organizationId;
            Roles = new HashSet<int>(roles ?? new[] { (int)UserRole.Customer });
            CreatedAt = DateTime.UtcNow;
            CreatedBy = userId ?? "self";
        }
        public void AddRole(int roleId) => Roles.Add(roleId);

        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.");

            Email = email;
        }

        public void UpdateName(string name) => Name = name;
        public void UpdatePhone(string phone) => Phone = phone;
        public void UpdateNric(string nricOrPassport) => NricOrPassport = nricOrPassport;
        public void UpdateOrganizationId(string organizationId) => OrganizationId = organizationId;

        public void ChangePassword(string hasher) => PasswordHash = hasher;


        public void Delete(string deletedBy)
        {
            if (IsDeleted) throw new InvalidOperationException("User already deleted.");
            
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        public void Restore(string restoredBy)
        {
            if (!IsDeleted)
                throw new InvalidOperationException("User is not deleted.");

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = restoredBy;
        }
    }
}
