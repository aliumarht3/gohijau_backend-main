using GoHijauBackend.Application.Hubs;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Services
{
    using GoHijauBackend.Application.Interfaces.Services;
    using Microsoft.AspNetCore.SignalR;

    public sealed class WithdrawalNotifierService(IHubContext<WithdrawalHub> hub) : IWithdrawalNotifierService
    {
        private readonly IHubContext<WithdrawalHub> _hub = hub;

        public Task NotifyChangedAsync(string userId, WithdrawalHistory wh)
            => _hub.Clients.Group($"user:{userId}").SendAsync("WithdrawalChanged", new
            {
                id = wh.Id,
                status = wh.Status,
                amount = wh.Amount,
                createdAt = wh.CreatedAt
            });

        public async Task NotifyChangedOwnerAsync(List<User> users, MachineOwnerWithdrawalHistory wh)
        {
            var tasks = users.Select(user =>
                hub.Clients.Group($"user:{user.Id}")
                   .SendAsync("WithdrawalChanged", new
                   {
                       id = wh.Id,
                       status = wh.Status,
                       amount = wh.Amount,
                       createdAt = wh.CreatedAt
                   })
            );

            await Task.WhenAll(tasks);
        }

    }
}
