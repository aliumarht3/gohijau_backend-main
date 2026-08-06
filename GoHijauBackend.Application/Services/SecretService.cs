using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using System.Transactions;


namespace GoHijauBackend.Application.Services
{
    public class SecretService : ISecretService
    {
        private readonly ISecretRepository _secretRepository;

        public SecretService(ISecretRepository secretRepository) 
        {
            _secretRepository = secretRepository;
        }
        public async Task<Result> CreateSecretKey(string secretKey, string keyId)
        {
            try
            {
                SecretKeys secret = new SecretKeys();
                secret.KeyId = keyId;
                secret.KeySecret = secretKey;
                var result = await _secretRepository.AddSecret(secret);
                return Result.Success();
            }
            catch (Exception ex) 
            {
                return Result.Failure(ex.Message);
            }
        }

        public async Task<Result<SecretKeysDto?>> GetSecret()
        {
            try 
            {
                var result = await _secretRepository.GetSecret();
                if (result == null) { return Result.Failure<SecretKeysDto?>("Secret not found"); }
                return Result.Success<SecretKeysDto?>(result);
            } 
            catch (Exception ex) 
            {
                return Result.Failure<SecretKeysDto?>("Secret not found");
            }
        }
    }
}
