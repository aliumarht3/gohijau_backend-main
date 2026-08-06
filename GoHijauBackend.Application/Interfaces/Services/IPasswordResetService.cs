using CSharpFunctionalExtensions;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IPasswordResetService
    {
        Task<Result> GenerateResetToken(string email, string source);
        Task<Result> ResetPassword(string token, string newPassword);
        Task<Result> ValidateResetToken(string token);
    }
}