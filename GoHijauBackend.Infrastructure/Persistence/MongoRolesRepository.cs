using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    internal class MongoRolesRepository : IRolesRepository
    {
        private readonly IMongoCollection<Role> _roles;
        public MongoRolesRepository(IMongoDatabase database)
        {
            _roles = database.GetCollection<Role>("Roles");
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roles.Find(_ => true).ToListAsync();
        }
    }
}
