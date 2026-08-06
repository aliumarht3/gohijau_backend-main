using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Persistence.External;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using GoHijauBackend.Domain.Entities.External;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Services
{
    public class MachineOwnerPayoutService : IMachineOwnerPayoutService
    {
        private readonly IMachineOwnerPayoutRepository _machineOwnerPayoutRepo;
        private readonly ICustomerPayoutGateway _payoutGateway;
        private readonly IBankAccountService _bankAccountService;
        private readonly IWithdrawalHistoryService _withdrawalHistoryService;
        private readonly TransactionService _transactionService;
        private readonly IMachineService _machineService;   
        private readonly IUserService _userService;
        private readonly IOrganizationRepository _organizationRepository;
        public MachineOwnerPayoutService(
            IMachineOwnerPayoutRepository machineOwnerPayoutRepo,
            ICustomerPayoutGateway payoutGateway,
            IBankAccountService bankAccountService, 
            IWithdrawalHistoryService withdrawalHistoryService,
            TransactionService transactionService,
            IMachineService machineService,
            IUserService userService,
            IOrganizationRepository organizationRepository
            )
        {
            _payoutGateway = payoutGateway;
            _machineOwnerPayoutRepo = machineOwnerPayoutRepo;
            _bankAccountService = bankAccountService;
            _withdrawalHistoryService = withdrawalHistoryService;
            _transactionService = transactionService;
            _machineService = machineService;
            _userService = userService;
            _organizationRepository = organizationRepository;
        }

        public async Task<PayoutStatus> GetPayoutStatus(string reference)
        {
            return await _payoutGateway.GetStatusAsync(reference);
        }
        public async Task<PayoutStatus> CreatePayout(PayoutRequest payoutRequest, string userId)
        {
            var user =await  _userService.GetUserById(userId);
            if (user == null)
            {
                return new PayoutStatus
                (
                    reference: "123",
                    payoutStatusCode: "400",
                    success: false,
                    rawResponse: "User not found"
                );
            }
            var organizationId = user.OrganizationId;
            bool hasEnoughPoints = await CheckUserRewardEnough(organizationId, payoutRequest.Amount);
            if (!hasEnoughPoints)
            {
                return new PayoutStatus
                (
                    reference: "123",
                    payoutStatusCode: "400",
                    success: false,
                    rawResponse: "Insufficient reward points for payout."
                );
            }

            var (isSuccess, error, withdrawalHistoryId) = await _withdrawalHistoryService.AddMachineOwnerWithdrawalHistory(organizationId, payoutRequest.Amount.ToString(),userId);
            var bankAccountResponse = await _bankAccountService.GetMachineOwnerBankAccount(organizationId);
            if (bankAccountResponse == null)
            {
                throw new Exception("No bank account found for this organization. Please contact support");
            }

            var payoutResponse = await _payoutGateway.CreatePayoutAsync(payoutRequest, withdrawalHistoryId, bankAccountResponse.BankCode, bankAccountResponse.AccountNumber,"MachineOwnerPayout");

            if (payoutResponse.Success)
            {
                var rawResponse = JsonDocument.Parse(payoutResponse.RawResponse);
                var recipientReference = rawResponse.RootElement.GetProperty("Response")[0].GetProperty("recipient_reference")[0].GetString();

                await Task.Delay(4000);
                var payoutStatusResponse = await GetPayoutStatus(recipientReference);

                var rawPayoutStatusResponse = JsonDocument.Parse(payoutStatusResponse.RawResponse);
                var payoutStatusCode = rawPayoutStatusResponse.RootElement.GetProperty("Response")[0][0].GetProperty("payout_status_code")[0].GetString();

                if (payoutStatusCode == "0")
                {
                    await _transactionService.DeductMachineOwnerTotalPoint(organizationId, Convert.ToDouble(payoutRequest.Amount));
                    await _withdrawalHistoryService.UpdateMachineOwnerWithdrawalStatus(withdrawalHistoryId, "SUCCESS");
                }
                else if (payoutStatusCode == "2" || payoutStatusCode == "7" || payoutStatusCode == "8" || payoutStatusCode == "11")
                {
                    await _withdrawalHistoryService.UpdateMachineOwnerWithdrawalStatus(withdrawalHistoryId, "REJECTED");
                }
                else
                {
                    await _transactionService.DeductMachineOwnerTotalPoint(organizationId, Convert.ToDouble(payoutRequest.Amount));
                    await _withdrawalHistoryService.UpdateMachineOwnerWithdrawalStatus(withdrawalHistoryId, "PENDING");
                }
            }
            return payoutResponse;
        }
        private async Task<bool> CheckUserRewardEnough(string organizationId, decimal amount)
        {
            var customerPoints = await _transactionService.GetTotalMachineOwnerRewardsByOrganizationId(organizationId);

            return customerPoints >= amount;
        }

        public async Task PopulateTotalTransactionMachineOwner(string organizationId)
        {
            MachineSearchRequest ms = new MachineSearchRequest();
            if (!string.IsNullOrEmpty(organizationId)) {
                ms.Owner = organizationId;
                List<Machine> machines = await _machineService.SearchMachines(ms);
                List<Transaction> transactions = new List<Transaction>();
                
                foreach (Machine machine in machines)
                {
                    List<Transaction> listtransmachine = await _transactionService.GetTransactionsByMachineId(machine.MachineId);
                    if (listtransmachine != null && listtransmachine.Count > 0)
                        transactions.AddRange(listtransmachine);
                }
                List<string> organizations = new List<string>();
                organizations.Add(organizationId);
                var organization = await _organizationRepository.GetOrganizationsByIds(organizations);
                var rate = 0.0;
                if (organization != null)
                {
                    foreach (var org in organization)
                    {
                        if (org.ProfitRate > 0)
                        {
                            rate = (double)org.ProfitRate;
                        }
                        else
                        {
                            rate = 0.9;
                        }
                    }
                }

                TotalMachineOwnerTransaction tmot = new TotalMachineOwnerTransaction
                {
                    OrganizationId = ms.Owner,
                    TotalOilCollected = transactions.Sum(t => t.OilPoured),
                    TotalCO2Saved = transactions.Sum(t => t.CO2Saved),
                    PointsAwarded = transactions.Sum(t => t.OilPoured) * rate
                };
                List<MachineOwnerWithdrawalHistory> wh = await _withdrawalHistoryService.GetWithdrawalHistoryBasedOnOrganizationId(ms.Owner)??new List<MachineOwnerWithdrawalHistory>();
                if (wh.Count > 0) {
                    decimal totalWithdrawn = wh
                        .Where(t => t.Status == "SUCCESS")
                        .Sum(t =>
                          decimal.TryParse(t.Amount, out var val) ? val : 0m
                      );
                    tmot.PointsAwarded -= (double)totalWithdrawn;
                }
                 await _transactionService.UpdateMachineOwnerTransactionTotalsAsync(tmot); 
            }
           

        }
    }
}
