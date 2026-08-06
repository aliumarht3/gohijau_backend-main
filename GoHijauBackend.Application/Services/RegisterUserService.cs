using GoHijauBackend.Domain.Entities;
using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Interfaces.Persistence;

namespace GoHijauBackend.Application.Services
{
    public class RegisterUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result> ExecuteAsync(string email, string name, string password, string phone)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                return Result.Failure("Email already in use");

            var hash = _passwordHasher.HashPassword(password);
            var user = new User(email, name, hash, phone, null, null, null);
            await _userRepository.AddAsync(user);

            return Result.Success();
        }
    }
}
