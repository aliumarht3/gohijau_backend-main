using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Services
{
    public class MachineOwnerWithdrawalHistoryService : IOwnerWithdrawawalHistoryService
    {
        private readonly IWithdrawalHistoryService _withdrawalHistoryService; 
        private readonly IUserService _userService; 

        public MachineOwnerWithdrawalHistoryService(IWithdrawalHistoryService withdrawalHistoryService, IUserService userService ) 
        {
            _withdrawalHistoryService = withdrawalHistoryService;
            _userService = userService; 
        }
        public async Task<List<MachineOwnerWithdrawalHistory>> GetWithdrawalHistory(string userId)
        {
            var user = await _userService.GetUserById(userId);
            if (user != null) 
            {
                return await _withdrawalHistoryService.GetWithdrawalHistoryBasedOnOrganizationId(user.OrganizationId);
            }
            return new List<MachineOwnerWithdrawalHistory>(); 
           
        }
    }
}
