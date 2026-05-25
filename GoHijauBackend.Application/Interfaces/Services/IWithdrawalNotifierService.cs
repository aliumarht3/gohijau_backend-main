using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IWithdrawalNotifierService
    {
        Task NotifyChangedAsync(string userId, WithdrawalHistory wh);
        Task NotifyChangedOwnerAsync(List<User>users, MachineOwnerWithdrawalHistory wh);
    }
}
