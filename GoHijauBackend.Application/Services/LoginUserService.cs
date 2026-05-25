using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class LoginUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokenRepo;

        public LoginUserService(IUserRepository repo, IPasswordHasher hasher, IJwtTokenGenerator tokenGen, IRefreshTokenRepository refreshTokenRepo)
        {
            _userRepository = repo;
            _passwordHasher = hasher;
            _tokenGenerator = tokenGen;
            _refreshTokenRepo = refreshTokenRepo;
        }

        public async Task<Result<TokenPair>> getAccessTokenUsingEmailPassword(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
                return Result.Failure<TokenPair>("Invalid credentials");

            return await generateTokens(user);
        }

        public async Task<Result<TokenPair>> getAccessTokenUsingRefreshToken(string refreshToken)
        {
            var tokenRecord = await _refreshTokenRepo.GetByTokenAsync(refreshToken);
            if (tokenRecord == null || tokenRecord.ExpiresAt < DateTime.UtcNow)
                return Result.Failure<TokenPair>("Invalid or expired refresh token");

            var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);

            if (user == null) 
                return Result.Failure<TokenPair>("User not found");

            await _refreshTokenRepo.DeleteAsync(refreshToken);

            return await generateTokens(user);

        }

        private async Task<Result<TokenPair>> generateTokens(User user)
        {
            var tokens = _tokenGenerator.GenerateTokens(user);
            await SaveRefreshToken(tokens, user);

            return Result.Success(tokens);
        }

        private async Task SaveRefreshToken(TokenPair tokens, User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = tokens.RefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenRepo.SaveAsync(refreshToken);
        }
    }
}
