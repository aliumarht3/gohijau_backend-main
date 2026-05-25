using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class ManageUserRoleService : IManageUserRoleService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRolesRepository _rolesRepository;

        public ManageUserRoleService(IUserRepository userRepository, IRolesRepository rolesRepository)
        {
            _userRepository = userRepository;
            _rolesRepository = rolesRepository;
        }

        public async Task Execute(string userId, int roleId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var validRoles = Enum.GetValues(typeof(UserRole)).Cast<int>().ToHashSet();

            if (!validRoles.Contains(roleId))
                throw new Exception("Invalid role ID provided.");

            user.AddRole(roleId);
            await _userRepository.UpdateAsync(user);
        }

        public async Task<List<Role>> GetAllRoles()
        {
            return await _rolesRepository.GetAllRolesAsync();
        }
    }
}
