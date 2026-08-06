using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Linq;

namespace GoHijauBackend.Background
{
    public class CollectionPostProcessQueue : ICollectionPostProcessQueue
    {
        private readonly IMongoCollection<CollectionPostProcessQueueItem> _queueCollection;
        private readonly ILogger<CollectionPostProcessQueue> _logger;

        public CollectionPostProcessQueue(
            IMongoClient mongoClient,
            IConfiguration configuration,
            ILogger<CollectionPostProcessQueue> logger)
        {
            var databaseName = configuration["MongoDB:Database"];
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("MongoDB database name is not configured.");
            }
            var database = mongoClient.GetDatabase(databaseName);
            _queueCollection = database.GetCollection<CollectionPostProcessQueueItem>("CollectionPostProcessQueue");
            _logger = logger;
        }

        public async ValueTask EnqueueAsync(CollectionPostProcessJob job, CancellationToken cancellationToken = default)
        {
            var queueItem = new CollectionPostProcessQueueItem
            {
                UserId = job.UserId,
                MachineId = job.MachineId,
                AccessToken = job.AccessToken,
                OilCollected = job.OilCollected,
                Status = "Pending",
                Attempts = 0,
                MaxAttempts = 3,
                NextAttemptAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _queueCollection.InsertOneAsync(queueItem, cancellationToken: cancellationToken);
        }

        public async ValueTask<CollectionPostProcessJob> DequeueAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var filter = Builders<CollectionPostProcessQueueItem>.Filter.And(
                    Builders<CollectionPostProcessQueueItem>.Filter.Eq(x => x.Status, "Pending"),
                    Builders<CollectionPostProcessQueueItem>.Filter.Lte(x => x.NextAttemptAt, now),
                    Builders<CollectionPostProcessQueueItem>.Filter.Where(x => x.Attempts < x.MaxAttempts)
                );

                var update = Builders<CollectionPostProcessQueueItem>.Update
                    .Set(x => x.Status, "Processing")
                    .Set(x => x.UpdatedAt, now);

                var options = new FindOneAndUpdateOptions<CollectionPostProcessQueueItem>
                {
                    ReturnDocument = ReturnDocument.After,
                    Sort = Builders<CollectionPostProcessQueueItem>.Sort.Ascending(x => x.CreatedAt)
                };

                var item = await _queueCollection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

                if (item != null)
                {
                    return new CollectionPostProcessJob
                    {
                        JobId = item.Id,
                        UserId = item.UserId,
                        MachineId = item.MachineId,
                        AccessToken = item.AccessToken,
                        OilCollected = item.OilCollected
                    };
                }

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            throw new OperationCanceledException(cancellationToken);
        }

        public async Task MarkCompletedAsync(string jobId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<CollectionPostProcessQueueItem>.Filter.Eq(x => x.Id, jobId);
            var update = Builders<CollectionPostProcessQueueItem>.Update
                .Set(x => x.Status, "Completed")
                .Set(x => x.UpdatedAt, DateTime.UtcNow)
                .Set(x => x.LastError, null);

            await _queueCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task MarkFailedAsync(string jobId, Exception exception, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var current = await _queueCollection
                .Find(x => x.Id == jobId)
                .FirstOrDefaultAsync(cancellationToken);

            if (current == null)
            {
                _logger.LogWarning("Collection post-process job not found while marking failure: {JobId}", jobId);
                return;
            }

            var nextAttempts = current.Attempts + 1;
            var isDead = nextAttempts >= current.MaxAttempts;
            var backoffSeconds = Math.Pow(2, Math.Min(nextAttempts, 6));

            var update = Builders<CollectionPostProcessQueueItem>.Update
                .Set(x => x.Attempts, nextAttempts)
                .Set(x => x.LastError, exception.Message)
                .Set(x => x.UpdatedAt, now)
                .Set(x => x.Status, isDead ? "Dead" : "Pending")
                .Set(x => x.NextAttemptAt, now.AddSeconds(backoffSeconds));

            await _queueCollection.UpdateOneAsync(x => x.Id == jobId, update, cancellationToken: cancellationToken);
        }

        public async Task<List<CollectionPostProcessQueueJobDto>> GetDeadJobsAsync(int limit = 50, CancellationToken cancellationToken = default)
        {
            if (limit <= 0)
            {
                limit = 50;
            }

            if (limit > 200)
            {
                limit = 200;
            }

            var deadJobs = await _queueCollection
                .Find(x => x.Status == "Dead")
                .SortByDescending(x => x.UpdatedAt)
                .Limit(limit)
                .ToListAsync(cancellationToken);

            return deadJobs.Select(x => new CollectionPostProcessQueueJobDto
            {
                JobId = x.Id,
                UserId = x.UserId,
                MachineId = x.MachineId,
                OilCollected = x.OilCollected,
                Attempts = x.Attempts,
                MaxAttempts = x.MaxAttempts,
                LastError = x.LastError,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();
        }

        public async Task<bool> ReplayDeadJobAsync(string jobId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var filter = Builders<CollectionPostProcessQueueItem>.Filter.And(
                Builders<CollectionPostProcessQueueItem>.Filter.Eq(x => x.Id, jobId),
                Builders<CollectionPostProcessQueueItem>.Filter.Eq(x => x.Status, "Dead")
            );

            var update = Builders<CollectionPostProcessQueueItem>.Update
                .Set(x => x.Status, "Pending")
                .Set(x => x.Attempts, 0)
                .Set(x => x.LastError, null)
                .Set(x => x.NextAttemptAt, now)
                .Set(x => x.UpdatedAt, now);

            var result = await _queueCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            return result.ModifiedCount > 0;
        }
    }
}
