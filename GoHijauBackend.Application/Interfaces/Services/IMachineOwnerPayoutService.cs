using GoHijauBackend.Domain.Entities.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IMachineOwnerPayoutService
    {
        Task<PayoutStatus> GetPayoutStatus(string reference);
        Task<PayoutStatus> CreatePayout(PayoutRequest payoutRequest, string userId);
        Task PopulateTotalTransactionMachineOwner(string userId);
    }
}
