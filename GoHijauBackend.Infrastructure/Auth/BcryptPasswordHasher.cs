using BCrypt.Net;
using GoHijauBackend.Application.Interfaces.Services;

namespace GoHijauBackend.Infrastructure.Auth
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string password, string hashed) =>
            BCrypt.Net.BCrypt.Verify(password, hashed);
    }
}
