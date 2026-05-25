using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IRolesRepository
    {
        Task<List<Role>> GetAllRolesAsync();
    }
}
