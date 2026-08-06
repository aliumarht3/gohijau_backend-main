using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;


namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface ISecretService
    {
        public Task<Result> CreateSecretKey(string secretKey, string keyId);
        public Task<Result<SecretKeysDto?>> GetSecret();
    }
}
