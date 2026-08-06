using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface ICollectionPostProcessQueue
    {
        ValueTask EnqueueAsync(CollectionPostProcessJob job, CancellationToken cancellationToken = default);
        ValueTask<CollectionPostProcessJob> DequeueAsync(CancellationToken cancellationToken);
        Task MarkCompletedAsync(string jobId, CancellationToken cancellationToken = default);
        Task MarkFailedAsync(string jobId, Exception exception, CancellationToken cancellationToken = default);
        Task<List<CollectionPostProcessQueueJobDto>> GetDeadJobsAsync(int limit = 50, CancellationToken cancellationToken = default);
        Task<bool> ReplayDeadJobAsync(string jobId, CancellationToken cancellationToken = default);
    }
}
