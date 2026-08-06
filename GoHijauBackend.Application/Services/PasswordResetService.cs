using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace GoHijauBackend.Application.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IPasswordResetRepository _resetRepo;
        private readonly IConfiguration _config;

        public PasswordResetService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IPasswordResetRepository resetRepo,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _resetRepo = resetRepo;
            _config = config;
        }

        public async Task<Result> GenerateResetToken(string email, string source)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return Result.Failure("If the email exists, you will receive reset instructions.");

            var token = CreateSecureTokenId();
            var entity = CreateResetTokenEntity(user.Id, token);
            await _resetRepo.CreateResetToken(entity);

            var (primaryLink, deepLink) = BuildResetUrlForSource(token, source);

            var sendResult = await _emailService.BuildAndSendPassswordResetEmail(user.Email, primaryLink, deepLink);
            if (!sendResult.IsSuccess)
            {
                return Result.Failure("Failed to send reset email.");
            }

            return Result.Success();
        }

        public async Task<Result> ResetPassword(string tokenId, string newPassword)
        {
            var tokenRecord = await _resetRepo.GetResetTokenByTokenId(tokenId);
            if (tokenRecord == null)
                return Result.Failure("Invalid or expired reset token.");

            var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);
            if (user == null)
            {
                await _resetRepo.DeleteResetTokenByTokenId(tokenId);
                return Result.Failure("User not found.");
            }

            user.ChangePassword(_passwordHasher.HashPassword(newPassword));
            user.ModifiedAt = DateTime.UtcNow;
            user.ModifiedBy = tokenRecord.UserId ?? "self";

            await _userRepository.UpdateAsync(user);

            await _resetRepo.DeleteResetTokenByTokenId(tokenId);

            return Result.Success();
        }

        private PasswordResetToken CreateResetTokenEntity(string userId, string tokenId)
        {
            return new PasswordResetToken
            {
                TokenId = tokenId,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        private (string PrimaryLink, string? DeepLink) BuildResetUrlForSource(string token, string source)
        {
            // source expected: "web" or "mobile". default to web when unknown.
            var normalized = source?.Trim()?.ToLowerInvariant() ?? "web";

            var frontendBase = _config["Frontend:BaseUrl"] ?? "https://dashboard.gohijau.org";
            var resetPath = _config["Frontend:ResetPath"] ?? "/reset-password";
            var webLink = $"{frontendBase.TrimEnd('/')}{resetPath}?token={Uri.EscapeDataString(token)}";

            if (string.Equals(normalized, "mobile", StringComparison.OrdinalIgnoreCase))
            {
                var mobileDeepLink = "gohijau://reset-password";
                var deepLink = $"{mobileDeepLink.TrimEnd('?', '/')}?token={Uri.EscapeDataString(token)}";
                return (webLink, deepLink);
            }

            return (webLink, null);
        }

        private static string CreateSecureTokenId()
        {
            var guidPart = Guid.NewGuid().ToString("N");
            var randomBytes = RandomNumberGenerator.GetBytes(16);
            var hex = Convert.ToHexString(randomBytes);
            return guidPart + hex;
        }

        public async Task<Result> ValidateResetToken(string tokenId)
        {
            if (string.IsNullOrWhiteSpace(tokenId))
                return Result.Failure("Token is required.");

            var tokenRecord = await _resetRepo.GetResetTokenByTokenId(tokenId);
            if (tokenRecord == null)
                return Result.Failure("Invalid or expired reset token.");

            return Result.Success();
        }
    }
}