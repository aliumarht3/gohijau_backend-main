using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<List<User>> GetFromOrganizationIdAsync(string id);
        Task<List<UserDisplayDto>> GetAllUsers();
        Task<User?> GetDeletedUserByEmailAsync(string email);
        Task<User> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(string id);
        Task<List<string>> GetDistinctOrganizationIdsByRole(UserRole role);
    }
}
