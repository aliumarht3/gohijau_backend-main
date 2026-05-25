using GoHijauBackend.Domain.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Infrastructure.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task SaveAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task DeleteAsync(string token);
    }
}