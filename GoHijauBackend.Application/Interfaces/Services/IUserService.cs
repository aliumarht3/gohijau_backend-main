using GoHijauBackend.Domain.Entities;
using GoHijauBackend.Application.Dto;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetUserById(string id);
        public Task<List<User>> GetUserFromOrganizationId(string organizationId);
        Task<User?> UpdateUser(string userId, UpdateProfileDto dto);
        Task<(bool IsSuccess, string? Error)> CreateUser(string userId, CreateUserCommand command);
        Task<List<UserDisplayDto>> GetAllUsers();
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<string> DeleteAccount(DeleteAccountDto dto, string currentUserId, bool isAdmin);
        Task<string?> RestoreUser(string userEmail, string adminId);
    }
}
