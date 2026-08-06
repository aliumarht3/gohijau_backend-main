using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface ICollectionPostProcessService
    {
        Task ProcessAsync(CollectionPostProcessJob job, CancellationToken cancellationToken = default);
    }
}
