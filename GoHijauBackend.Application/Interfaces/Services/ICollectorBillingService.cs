using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface ICollectorBillingService
    {
        public Task<Result<CollectorBillDto?>> GetBill(string userId);
    }
}
