using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;
using System.Reflection.PortableExecutable;


namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoMachineUCOTrackingRepository : IMachineUCOTrackingRepository
    {
        private readonly IMongoCollection<MachineUCOTracking> _collectionUCOTracking;
        private readonly IMongoCollection<Domain.Entities.Machine> _machine;
        private readonly IMongoCollection<User> _users;
        public MongoMachineUCOTrackingRepository(IMongoDatabase database)
        {
            _collectionUCOTracking = database.GetCollection<MachineUCOTracking>("MachineUCOTrackings");
            _machine = database.GetCollection<Domain.Entities.Machine>("Machines");
            _users = database.GetCollection<User>("users");

        }
        public async Task<Result<MachineUCOTracking>> AddMachineTrackingBuffer(MachineUCOTracking uCOTracking)
        {
            try
            {
                await _collectionUCOTracking
                    .WithWriteConcern(WriteConcern.WMajority)
                    .InsertOneAsync(uCOTracking);

                return Result.Success(uCOTracking);
            }
            catch (MongoWriteException mwx) when (mwx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return Result.Failure<MachineUCOTracking>("Insert failed: duplicate key.");
            }
            catch (Exception ex)
            {
                return Result.Failure<MachineUCOTracking>($"Insert failed: {ex.Message}");
            }
        }

        public async Task<bool> CheckMachineExists(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                return false;

            var filter = Builders<MachineUCOTracking>.Filter.Eq(t => t.MachineId, machineId);
            return await  _collectionUCOTracking
                .Find(filter)
                .Limit(1)
                .AnyAsync();
        }



        public async Task<List<MachineUCOTracking>> GetAllUCOTracker()
        {
                    return await _collectionUCOTracking
              .Find(FilterDefinition<MachineUCOTracking>.Empty)
              .ToListAsync();
        }

        public async Task<List<MachineUCOTracking>> GetUCOTrackerByCollectorId(string collectorId)
        {
            if (string.IsNullOrWhiteSpace(collectorId))
                return new List<MachineUCOTracking>();
            var collector = await _users.Find(m=>m.Id == collectorId).FirstOrDefaultAsync();
            if (collector == null) return new List<MachineUCOTracking>();
            var machineIds = await _machine
                .Find(m => m.Collector == collector.OrganizationId)
                .Project(m => m.MachineId)
                .ToListAsync();

            if (machineIds.Count == 0)
                return new List<MachineUCOTracking>();

            var filter = Builders<MachineUCOTracking>.Filter.In(t => t.MachineId, machineIds.Distinct());
            return await _collectionUCOTracking
                .Find(filter)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<MachineUCOTracking> GetUCOTrackerByMachineId(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId))
                return new MachineUCOTracking();

            return await _collectionUCOTracking.Find(x=> x.MachineId == machineId).FirstOrDefaultAsync();
        }

        public async Task<List<MachineUCOTracking>> GetUCOTrackerByOwnerId(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return new List<MachineUCOTracking>();
            var owner = await _users.Find(m => m.Id == ownerId).FirstOrDefaultAsync();
            if (owner == null) return new List<MachineUCOTracking>();
            var machineIds = await _machine
                .Find(m => m.Owner == owner.OrganizationId)
                .Project(m => m.MachineId)
                .ToListAsync();

            if (machineIds.Count == 0)
                return new List<MachineUCOTracking>();

            var filter = Builders<MachineUCOTracking>.Filter.In(t => t.MachineId, machineIds.Distinct());
            return await _collectionUCOTracking
                .Find(filter)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Result<MachineUCOTracking>> UpdateUCOTracker(MachineUCOTracking uCOTracking)
        {
            if (uCOTracking is null)
                return Result.Failure<MachineUCOTracking>("Payload is null.");
            if (string.IsNullOrWhiteSpace(uCOTracking.Id))
                return Result.Failure<MachineUCOTracking>("Missing Id on UCOTracking.");

            try
            {
                var filter = Builders<MachineUCOTracking>.Filter.Eq(x => x.Id, uCOTracking.Id);
                var options = new ReplaceOptions { IsUpsert = false };

                var result = await _collectionUCOTracking
                    .WithWriteConcern(WriteConcern.WMajority)
                    .ReplaceOneAsync(filter, uCOTracking, options);
                var fetchedTracking = await _collectionUCOTracking.Find(filter).FirstOrDefaultAsync();
                if (result.MatchedCount == 0)
                    return Result.Failure<MachineUCOTracking>("UCOTracking not found.");

                return Result.Success<MachineUCOTracking>(fetchedTracking);
            }
            catch (MongoWriteConcernException wcx)
            {
                return Result.Failure<MachineUCOTracking>($"Update failed: write concern not satisfied ({wcx.Message}).");
            }
            catch (MongoWriteException mwx)
            {
                return Result.Failure<MachineUCOTracking>($"Update failed: {mwx.Message}");
            }
            catch (OperationCanceledException)
            {
                return Result.Failure<MachineUCOTracking>("Update cancelled.");
            }
            catch (Exception ex)
            {
                return Result.Failure<MachineUCOTracking>($"Update failed: {ex.Message}");
            }
        }
    }
}
