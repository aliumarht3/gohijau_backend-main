using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface ISecretRepository
    {
        Task<Result> AddSecret(SecretKeys secretKeys);
        Task<SecretKeysDto?> GetSecret();
    }
}
