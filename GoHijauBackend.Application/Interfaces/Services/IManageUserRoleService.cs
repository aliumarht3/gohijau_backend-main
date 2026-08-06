using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IManageUserRoleService
    {
        Task Execute(string userId, int roleId);
        Task<List<Role>> GetAllRoles();
    }
}
