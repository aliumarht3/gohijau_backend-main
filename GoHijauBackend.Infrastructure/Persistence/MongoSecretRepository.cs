using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Infrastructure.Persistence
{
    public class MongoSecretRepository : ISecretRepository
    {
        private readonly IMongoCollection<SecretKeys> _secret;

        public MongoSecretRepository(IMongoDatabase database) 
        {
            _secret = database.GetCollection<SecretKeys>("SecretKeys");
        }
        public async Task<Result> AddSecret(SecretKeys secret)
        {
            await _secret.InsertOneAsync(secret);
            return Result.Success(); 
        }

        public async Task<SecretKeysDto?> GetSecret()
        {
            try 
            {
                var entity = await _secret
                            .Find(FilterDefinition<SecretKeys>.Empty)
                            .SortByDescending(x => x.CreatedAt)
                            .FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                return new SecretKeysDto
                {
                    KeyId = entity.KeyId,
                    KeySecret = entity.KeySecret,
                };
            } 
            catch (Exception ex) 
            {
                return null; 
            }
         
        }
    }
}
