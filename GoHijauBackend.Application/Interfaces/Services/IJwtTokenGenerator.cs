using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public record TokenPair(string AccessToken, string RefreshToken);
    public interface IJwtTokenGenerator
    {
        TokenPair GenerateTokens(User user);
    }
}