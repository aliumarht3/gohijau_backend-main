using GoHijauBackend.Application.Interfaces.Services;

namespace GoHijauBackend.Background
{
    public class CollectionPostProcessWorker : BackgroundService
    {
        private readonly ICollectionPostProcessQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CollectionPostProcessWorker> _logger;

        public CollectionPostProcessWorker(
            ICollectionPostProcessQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<CollectionPostProcessWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = await _queue.DequeueAsync(stoppingToken);
                    await ProcessAsync(job, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while processing collection post-process queue.");
                }
            }
        }

        private async Task ProcessAsync(
            GoHijauBackend.Application.Dto.CollectionPostProcessJob job,
            CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ICollectionPostProcessService>();
                await processor.ProcessAsync(job, cancellationToken);
                await _queue.MarkCompletedAsync(job.JobId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Collection post-process failed for job {JobId}, user {UserId}, machine {MachineId}.",
                    job.JobId,
                    job.UserId,
                    job.MachineId);

                await _queue.MarkFailedAsync(job.JobId, ex, cancellationToken);
            }
        }
    }
}
