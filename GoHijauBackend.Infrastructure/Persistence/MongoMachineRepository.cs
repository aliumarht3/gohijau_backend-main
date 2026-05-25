using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using GoHijauBackend.Application.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoMachineRepository : IMachineRepository
    {
        private readonly IMongoCollection<Machine> _collection;
        private readonly IMongoCollection<User> _user;

        public MongoMachineRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Machine>("Machines");
            _user = database.GetCollection<User>("users");
        }

        public async Task<List<Machine>> GetAllMachinesAsync()
        {
            return await _collection
                .Find(x => x.Status != MachineStatus.DELETED)
                .ToListAsync();
        }
        public async Task<List<Machine>> GetAllOwnerMachinesAsync(string userId)
        {
            var user = await _user.Find(m => m.Id == userId).FirstOrDefaultAsync();
            return await _collection
                .Find(m => m.Owner == user.OrganizationId)
                .ToListAsync();
        }

        public async Task<List<Machine>> GetMachinesByCollectorOrganizationId(string organizationId)
        {
            return await _collection
                .Find(m => m.Collector == organizationId && m.Status != MachineStatus.DELETED)
                .ToListAsync();
        }

        public async Task AddAsync(Machine machine) =>
            await _collection.InsertOneAsync(machine);

        public async Task<Machine?> GetByIdAsync(string id) =>
            await _collection.Find(x => x.Id == id && x.Status != MachineStatus.DELETED)
            .FirstOrDefaultAsync();
        public async Task<Machine?> GetByMachineIdAsync(string id) =>
            await _collection.Find(x => x.MachineId == id && x.Status != MachineStatus.DELETED)
            .FirstOrDefaultAsync();

        public async Task<bool> ExistsAsync(string machineId)
        {
            return await _collection.Find(m => m.MachineId == machineId).AnyAsync();
        }

        public async Task UpdateAsync(Machine machine) =>
            await _collection.ReplaceOneAsync(x => x.Id == machine.Id, machine);

        public async Task<List<Machine>> SearchMachinesAsync(MachineSearchRequest request)
        {
            var filterBuilder = Builders<Machine>.Filter;
            var filters = new List<FilterDefinition<Machine>>();

            // Exclude soft deleted unless explicitly searching for it
            if (request.Status == null || request.Status != MachineStatus.DELETED)
                filters.Add(filterBuilder.Ne(x => x.Status, MachineStatus.DELETED));

            // Partial matches (case-insensitive regex)
            if (!string.IsNullOrEmpty(request.Name))
                filters.Add(filterBuilder.Regex(x => x.Location.Name, new BsonRegularExpression(request.Name, "i")));
            if (!string.IsNullOrEmpty(request.UnitNo))
                filters.Add(filterBuilder.Regex(x => x.Location.UnitNo, new BsonRegularExpression(request.UnitNo, "i")));
            if (!string.IsNullOrEmpty(request.Street))
                filters.Add(filterBuilder.Regex(x => x.Location.Street, new BsonRegularExpression(request.Street, "i")));
            if (!string.IsNullOrEmpty(request.District))
                filters.Add(filterBuilder.Regex(x => x.Location.District, new BsonRegularExpression(request.District, "i")));
            if (!string.IsNullOrEmpty(request.Postcode))
                filters.Add(filterBuilder.Regex(x => x.Location.Postcode, new BsonRegularExpression(request.Postcode, "i")));
            if (!string.IsNullOrEmpty(request.State))
                filters.Add(filterBuilder.Regex(x => x.Location.State, new BsonRegularExpression(request.State, "i")));
            if (!string.IsNullOrEmpty(request.Country))
                filters.Add(filterBuilder.Regex(x => x.Location.Country, new BsonRegularExpression(request.Country, "i")));

            if (!string.IsNullOrEmpty(request.Coordinate))
                filters.Add(filterBuilder.Eq(x => x.Location.Coordinates, request.Coordinate));

            // Coordinates (optional exact match — could extend to range / geo query later)
            //if (request.Latitude.HasValue && request.Longitude.HasValue)
            //    filters.Add(filterBuilder.And(
            //        filterBuilder.Eq(x => x.Location.Coordinates, request.Latitude.Value)
            //        //filterBuilder.Eq(x => x.Location.Longitude, request.Longitude.Value)
            //    ));

            if (request.Type.HasValue)
                filters.Add(filterBuilder.Eq(x => x.Type, request.Type.Value));

            if (request.Status.HasValue)
                filters.Add(filterBuilder.Eq(x => x.Status, request.Status.Value));

            if (!string.IsNullOrEmpty(request.Owner))
                filters.Add(filterBuilder.Eq(x => x.Owner, request.Owner));

            // Date range filter
            if (request.ManufacturedDateFrom.HasValue)
                filters.Add(filterBuilder.Gte(x => x.ManufacturedDate, request.ManufacturedDateFrom.Value));
            if (request.ManufacturedDateTo.HasValue)
                filters.Add(filterBuilder.Lte(x => x.ManufacturedDate, request.ManufacturedDateTo.Value));

            var finalFilter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;

            return await _collection.Find(finalFilter).ToListAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
