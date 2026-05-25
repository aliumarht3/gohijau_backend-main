using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public MongoUserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
        }

        public Task<User> GetByEmailAsync(string email) =>
            _users.Find(u => u.Email == email && !u.IsDeleted).FirstOrDefaultAsync();

        public Task AddAsync(User user) =>
            _users.InsertOneAsync(user);

        public async Task<List<UserDisplayDto>> GetAllUsers()
        {
            var pipeline = _users.Aggregate()

                // Lookup user transactions (Role 0)
                .AppendStage<BsonDocument>(LookupUserTransactions())

                // Lookup machine owner transactions (Role 2)
                .AppendStage<BsonDocument>(LookupMachineOwnerTransactions())

                // Role-based points logic
                .AppendStage<BsonDocument>(AddPointsLogic())

                // Remove temporary fields
                .Project<UserJoined>(Builders<BsonDocument>.Projection
                    .Exclude("OrganizationObjectId")
                    .Exclude("UserTransactions")
                    .Exclude("MachineOwnerTransactions"));

            var users = await pipeline
                .As<UserJoined>()
                .ToListAsync();

            var result = users.Select(u => new UserDisplayDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                Phone = u.Phone,
                NricOrPassport = u.NricOrPassport,
                OrganizationId = u.OrganizationId,
                OrganizationName = u.OrganizationData.FirstOrDefault()?.OrganizationName ?? "",
                PointsAwarded = u.PointsAwarded,
                RolesId = u.Roles,
                Roles = u.Roles.Select(r => Enum.GetName(typeof(UserRole), r) ?? "Unknown").ToList()
            }).ToList();

            return result;
        }

        private BsonDocument LookupUserTransactions()
        {
            return new BsonDocument("$lookup",
                new BsonDocument
                {
            { "from", "TotalTransactions" },
            { "let", new BsonDocument("userId", "$_id") },
            { "pipeline", new BsonArray
                {
                    new BsonDocument("$match",
                        new BsonDocument("$expr",
                            new BsonDocument("$eq", new BsonArray
                            {
                                "$UserId",
                                new BsonDocument("$toString", "$$userId")
                            })
                        )
                    ),
                    new BsonDocument("$project",
                        new BsonDocument("PointsAwarded", 1))
                }
            },
            { "as", "UserTransactions" }
                });
        }

        private BsonDocument LookupMachineOwnerTransactions()
        {
            return new BsonDocument("$lookup",
                new BsonDocument
                {
            { "from", "TotalMachineOwnerTransactions" },
            { "let", new BsonDocument("orgId", "$OrganizationId") },
            { "pipeline", new BsonArray
                {
                    new BsonDocument("$match",
                        new BsonDocument("$expr",
                            new BsonDocument("$eq", new BsonArray
                            {
                                "$OrganizationId",
                                "$$orgId"
                            })
                        )
                    ),
                    new BsonDocument("$project",
                        new BsonDocument("PointsAwarded", 1))
                }
            },
            { "as", "MachineOwnerTransactions" }
                });
        }

        private BsonDocument AddPointsLogic()
        {
            return new BsonDocument("$addFields",
                new BsonDocument("PointsAwarded",
                    new BsonDocument("$cond", new BsonDocument
                    {
                { "if", new BsonDocument("$in", new BsonArray { 0, "$Roles" }) },
                { "then", new BsonDocument("$sum", "$UserTransactions.PointsAwarded") },
                { "else", new BsonDocument("$cond", new BsonDocument
                    {
                        { "if", new BsonDocument("$in", new BsonArray { 2, "$Roles" }) },
                        { "then", new BsonDocument("$sum", "$MachineOwnerTransactions.PointsAwarded") },
                        { "else", 0 }
                    })
                }
                    })
                ));
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id && !u.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task<User?> GetDeletedUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email && u.IsDeleted).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task DeleteAsync(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0)
            {
                throw new InvalidOperationException("User not found.");
            }
        }

        public async Task<List<string>> GetDistinctOrganizationIdsByRole(UserRole role)
        {
            var roleValue = (int)role;

            var filter = Builders<User>.Filter.AnyEq(u => u.Roles, roleValue);

            return await _users
                .Distinct<string>("OrganizationId", filter)
                .ToListAsync();
        }

        public async Task<List<User>> GetFromOrganizationIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new List<User>();

            return await _users
                .Find(u => u.OrganizationId == id && !u.IsDeleted)
                .ToListAsync();
        }
    }
}
