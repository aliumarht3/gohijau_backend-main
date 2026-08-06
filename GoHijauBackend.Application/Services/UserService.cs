using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class UserService(IUserRepository userRepository, IPasswordHasher _passwordHasher, IEmailService emailService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordHasher _passwordHasher = _passwordHasher;
        private readonly IEmailService _emailService = emailService;

        public async Task<User?> GetUserById(string userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }
        public async Task<List<User>> GetUserFromOrganizationId(string organizationId)
        {
            return await _userRepository.GetFromOrganizationIdAsync(organizationId);
        }

        public async Task<List<UserDisplayDto>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task<(bool IsSuccess, string? Error)> CreateUser(string userId, CreateUserCommand command)
        {
            var existing = await _userRepository.GetByEmailAsync(command.Email);
            if (existing != null)
                return (false, "Email already exists.");

            var validRoles = Enum.GetValues(typeof(UserRole)).Cast<int>().ToHashSet();

            if (command.RoleId == null || !command.RoleId.All(r => validRoles.Contains(r)))
            {
                return (false, "Invalid role ID(s) provided.");
            }

            var generatedPassword = GenerateSecurePassword(12);

            var hashPassword = _passwordHasher.HashPassword(generatedPassword);
            var user = new User
            (
                command.Email,
                command.Name,
                hashPassword,
                command.Phone,
                command.NricOrPassport,
                command.OrganizationId,
                userId,
                command.RoleId
            );

            await _userRepository.AddAsync(user);

            var emailResult = await _emailService.BuildAndSendNewAccountEmail(user.Email, generatedPassword);
            return (true, null);
        }

        private static string GenerateSecurePassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-+=<>?";
            var random = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => validChars[random.Next(validChars.Length)]).ToArray());
        }
        public async Task<User?> UpdateUser(string userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                    throw new InvalidOperationException("Email is already in use.");

                user.UpdateEmail(dto.Email);
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.UpdateName(dto.Name);

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                user.UpdatePhone(dto.Phone);

            if (!string.IsNullOrWhiteSpace(dto.NricOrPassport))
                user.UpdateNric(dto.NricOrPassport);

            if (!string.IsNullOrWhiteSpace(dto.OrganizationId))
                user.UpdateOrganizationId(dto.OrganizationId);

            await _userRepository.UpdateAsync(user);

            return user;
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            // Verify current password
            var isValid = _passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash);
            if (!isValid)
                throw new InvalidOperationException("Current password is incorrect.");

            user.ChangePassword(_passwordHasher.HashPassword(dto.NewPassword));

            await _userRepository.UpdateAsync(user);
        }

        public async Task<string> DeleteAccount(DeleteAccountDto dto, string currentUserId, bool isAdmin)
        {
            if (isAdmin && !string.IsNullOrEmpty(dto.TargetUserEmail))
            {
                await DeleteAccountByAdmin(dto.TargetUserEmail, currentUserId);
                return "User account deleted by admin.";
            }
            else
            {
                if (string.IsNullOrEmpty(dto.Password))
                    throw new InvalidOperationException("Password is required to delete your own account.");

                await DeleteOwnAccount(currentUserId, dto.Password);
                return "Account Deleted Successfully.";
            }
        }

        private async Task DeleteOwnAccount(string userId, string password)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            var isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);
            if (!isValid)
                throw new InvalidOperationException("Password is incorrect.");

            user.Delete("self");

            await _userRepository.UpdateAsync(user); 
        }

        private async Task DeleteAccountByAdmin(string targetUserEmail, string adminId)
        {
            var user = await _userRepository.GetByEmailAsync(targetUserEmail);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.Delete(adminId);

            await _userRepository.UpdateAsync(user);
        }

        public async Task<string?> RestoreUser(string userEmail, string adminId)
        {
            var user = await _userRepository.GetDeletedUserByEmailAsync(userEmail);
            if (user == null)
                throw new InvalidOperationException("User not found or not deleted.");

            user.Restore(adminId);
            await _userRepository.UpdateAsync(user);

            return user.Email;
        }
    }
}