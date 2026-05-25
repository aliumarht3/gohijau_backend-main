using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Interfaces.Services.ActivityLogs;
using GoHijauBackend.Application.Requests;
using GoHijauBackend.Domain.Entities;
using MongoDB.Driver;
namespace GoHijauBackend.Application.Services
{
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITotalTransactionRepository _totalTransactionRepository;
        private readonly IMachineRepository _machineRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMongoClient _mongoClient;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoicePdfService _invoicePdfService;
        private readonly IEmailService _emailService;
        private readonly IOrganizationService _organizationService;
        private readonly IMachineOwnerProfitDebtLogService _machineOwnerProfitDebtLogService;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ITotalTransactionRepository totalTransactionRepository,
            IMachineRepository machineRepository,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IInvoiceService invoiceService,
            IInvoicePdfService invoicePdfService,
            IEmailService emailService,
            IMongoClient mongoClient,
            IOrganizationService organizationService,
            IMachineOwnerProfitDebtLogService machineOwnerProfitDebtLogService)
        {
            _transactionRepository = transactionRepository;
            _totalTransactionRepository = totalTransactionRepository;
            _machineRepository = machineRepository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository; 
            _mongoClient = mongoClient;
            _invoiceService = invoiceService;
            _invoicePdfService = invoicePdfService;
            _emailService = emailService;
            _organizationService = organizationService;
            _machineOwnerProfitDebtLogService = machineOwnerProfitDebtLogService;
        }

        public async Task RecordTransactionAsync(RecordTransactionRequest request)
        {
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var machine = await _machineRepository.GetByMachineIdAsync(request.MachineId);
                var organization = await _organizationRepository.GetOrganizationById(machine.Owner);

                var transaction = new Transaction
                {
                    UserId = request.UserId,
                    OilPoured = request.OilPoured,
                    CO2Saved = request.CO2Saved,
                    PointsAwarded = organization.CustomerRate > 0 ?request.OilPoured * organization.CustomerRate.Value : 1.2* request.OilPoured,
                    MachineId = request.MachineId,
                    AccessToken = request.AccessToken,
                    CreatedAt = DateTime.UtcNow
                };
                transaction = await _transactionRepository.InsertTransactionAsync(transaction, session);

                await _totalTransactionRepository.UpdateTotalsAsync(
                    transaction.UserId,
                    transaction.OilPoured,
                    transaction.CO2Saved,
                    transaction.PointsAwarded,
                    transaction.AccessToken,
                    session
                );
                decimal profitRate = 0;
                decimal debt = 0;

                if (organization != null)
                {
                    profitRate = organization.ProfitRate > 0
                        ? Convert.ToDecimal(organization.ProfitRate)
                        : 0.9m;

                    debt = Convert.ToDecimal(organization.OutstandingDebt ?? 0);
                }

                decimal awardedPoints = Convert.ToDecimal(transaction.OilPoured) * profitRate;

                decimal obtainedPoints = await HandleDebt(
                    request,
                    transaction,
                    machine,
                    profitRate,
                    debt,
                    awardedPoints,
                    session
                );

                var updatedTotalMachineOwnerTransaction = await _totalTransactionRepository.UpdateMachineOwnerTotalsAsync
                    (
                        machine.Owner,
                        transaction.OilPoured,
                        transaction.CO2Saved,
                        Convert.ToDouble(obtainedPoints),
                        transaction.AccessToken,
                        session
                    );

                await session.CommitTransactionAsync();
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        private async Task<decimal> HandleDebt(
            RecordTransactionRequest request,
            Transaction transaction,
            Machine machine,
            decimal rate,
            decimal debt,
            decimal points,
            IClientSessionHandle session
        )
        {
            decimal currentEwalletBalance = await GetTotalMachineOwnerRewardsByOrganizationId(machine.Owner);
            if (debt > 0)
            {
                if (debt >= points)
                {
                    debt = debt - points;
                    await _machineOwnerProfitDebtLogService.ReduceDebtLog
                    (
                        request.UserId,
                        new MachineOwnerLogCommand
                        {
                            CustomerTransactionId = transaction.Id,
                            MachineOwnerOrganizationId = machine.Owner,
                            MachineId = request.MachineId,
                            CustomerId = request.UserId,
                            UcoWeight = request.OilPoured,
                            ProfitRate = Convert.ToDouble(rate),
                            Amount = points,
                            NewEwalletBalance = currentEwalletBalance,
                            NewDebtBalance = debt
                        }, 
                        session
                    );
                    points = 0;
                }
                else
                {
                    decimal debtPaid = debt;
                    decimal remainingPoints = points - debt;

                    await _machineOwnerProfitDebtLogService.ReduceDebtLog
                    (
                        request.UserId,
                        new MachineOwnerLogCommand
                        {
                            CustomerTransactionId = transaction.Id,
                            MachineOwnerOrganizationId = machine.Owner,
                            MachineId = request.MachineId,
                            CustomerId = request.UserId,
                            UcoWeight = request.OilPoured,
                            ProfitRate = Convert.ToDouble(rate),
                            Amount = debtPaid,
                            NewEwalletBalance = currentEwalletBalance,
                            NewDebtBalance = 0
                        }, 
                        session
                    );

                    currentEwalletBalance += remainingPoints;

                    await _machineOwnerProfitDebtLogService.AddEwalletLog
                    (
                        request.UserId,
                        new MachineOwnerLogCommand
                        {
                            CustomerTransactionId = transaction.Id,
                            MachineOwnerOrganizationId = machine.Owner,
                            MachineId = request.MachineId,
                            CustomerId = request.UserId,
                            UcoWeight = request.OilPoured,
                            ProfitRate = Convert.ToDouble(rate),
                            Amount = remainingPoints,
                            NewEwalletBalance = currentEwalletBalance,
                            NewDebtBalance = 0
                        },
                        session
                    );

                    debt = 0;
                    points = remainingPoints;
                }

                await _organizationService.UpdateOrganizationOutstandingDebt
                (
                    request.UserId, 
                    machine.Owner,
                    debt,
                    session
                );
            }
            else
            {
                currentEwalletBalance += points;

                var result = await _machineOwnerProfitDebtLogService.AddEwalletLog
                (
                    request.UserId,
                    new MachineOwnerLogCommand
                    {
                        CustomerTransactionId = transaction.Id,
                        MachineOwnerOrganizationId = machine.Owner,
                        MachineId = request.MachineId,
                        CustomerId = request.UserId,
                        UcoWeight = request.OilPoured,
                        ProfitRate = Convert.ToDouble(rate),
                        Amount = points,
                        NewEwalletBalance = currentEwalletBalance,
                        NewDebtBalance = 0
                    },
                    session
                );

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"AddEwalletLog failed: {result.Error}");
                    throw new Exception($"Failed to add ewallet log: {result.Error}");
                }
            }

            return points;
        }

        public async Task InsertCollectorRecordedUCOTransactionAsync(string userId, double collectorRecordedOil)
        {
                var newRecord = new CollectorTransaction
                {
                    UserId = userId,
                    CollectorRecordedOil = collectorRecordedOil,
                    OilCollected = 0,
                    CO2Saved = 0,
                    MachineId = null,
                    AccessToken = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _transactionRepository.InsertCollectorTransactionAsync(newRecord);
        }

        public async Task UpdateCollectorTransactionAsync(RecordCollectorTransactionRequest request)
        {
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var transaction = new CollectorTransaction
                {
                    UserId = request.UserId,
                    OilCollected = request.OilCollected,
                    CO2Saved = request.CO2Saved,
                    AccessToken = request.AccessToken,
                    MachineId = request.MachineId,
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = request.MachineId
                };

                await _transactionRepository.UpdateCollectorTransactionAsync(transaction, session);

                await _totalTransactionRepository.UpdateCollectorTotalsAsync(
                    transaction.UserId,
                    transaction.OilCollected,
                    transaction.CO2Saved,
                    transaction.AccessToken,
                    session
                );

                await session.CommitTransactionAsync();
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
        public async Task UpdateMachineOwnerTransactionTotalsAsync(TotalMachineOwnerTransaction request)
        {
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                await _totalTransactionRepository.UpdateMachineOwnerTotalsAsync(
                    request.OrganizationId,
                    request.TotalOilCollected,
                    request.TotalCO2Saved,
                    request.PointsAwarded,
                    request.AccessToken,
                    session
                );

                await session.CommitTransactionAsync();
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task<List<Transaction>> GetTransactionsByUserId(string userId)
        {
            return await _transactionRepository.GetByUserIdAsync(userId);
        }
        public async Task<List<Transaction>> GetTransactionsByMachineId(string machineId)
        {
            return await _transactionRepository.GetByMachineIdAsync(machineId);
        }
        public async Task<double> GetAdminTotalOilPoured()
        {
            return await _transactionRepository.GetAdminTotalOilPoured();
        }
        public async Task<double> GetOwnerTotalOilPoured(string userId)
        {
            return await _transactionRepository.GetOwnerTotalOilPoured(userId);
        }
        public async Task<List<CollectorTransaction>> GetCollectorTransactionsByUserId(string userId)
        {
            return await _transactionRepository.GetTransactionsByCollectorId(userId);
        }

        public async Task<decimal> GetTotalCustomerRewardsByUserId(string userId)
        {
            return await _totalTransactionRepository.GetCustomerTotalPointsAwarded(userId);
        }
        public async Task<decimal> GetTotalMachineOwnerRewardsByOrganizationId(string organizationId)
        {
            return await _totalTransactionRepository.GetMachineOwnerTotalPointsAwarded(organizationId);
        }

        public async Task<TotalTransaction?> GetTotalTransactionByUserId(string userId)
        {
            return await _totalTransactionRepository.GetByUserIdAsync(userId);
        }
        public async Task<TotalMachineOwnerTransaction?> GetTotalMachineOwnerTransactionByOrganizationId(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                return await _totalTransactionRepository.GetByOrganizationIdAsync(user.OrganizationId);
            }
            return null; 
            
        }

        public async Task<(bool IsSuccess, string? Error)> DeductCustomerTotalPoint(string userId, double amount)
        {
            try
            {
                var res = await _totalTransactionRepository.DeductCustomerTotalPoint(userId, amount);
                return (res, null);
            }
            catch (Exception e)
            {
                return (false, "Something went wrong. Please try again");
            }
        }
        public async Task<(bool IsSuccess, string? Error)> DeductMachineOwnerTotalPoint(string organizationId, double amount)
        {
            try
            {
                var res = await _totalTransactionRepository.DeductMachineOwnerTotalPoint(organizationId, amount);
                return (res, null);
            }
            catch (Exception e)
            {
                return (false, "Something went wrong. Please try again");
            }
        }

        public async Task<Transaction?> GetTransactionByTokenAsync(string accessToken)
        {
            return await _transactionRepository.GetTransactionsByAccessTokenAsync(accessToken);
        }

        public async Task<CollectorTransaction?> GetCollectorTransactionByTokenAsync(string accessToken)
        {
            return await _transactionRepository.GetCollectorTransactionsByAccessTokenAsync(accessToken);
        }

        public async Task<List<CollectorHistoryDto>> GetCollectorHistoryAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new List<CollectorHistoryDto>();
            }

            var userDict = new Dictionary<string, User>();
            var userOrgInfoDict = new Dictionary<string, (string OrganizationName, double? CollectorRate)>();

            bool isAdmin = user.Roles.Contains((int)UserRole.Admin);

            if (isAdmin)
            {
                var allOrgIds = await _userRepository.GetDistinctOrganizationIdsByRole(UserRole.OilCollector);
                foreach (var orgId in allOrgIds)
                {
                    var orgUsers = await _userRepository.GetFromOrganizationIdAsync(orgId);
                    var organization = await _organizationRepository.GetOrganizationById(orgId);
                    var orgName = organization?.OrganizationName ?? "Unknown Organization";
                    var collectorRate = organization?.CollectorRate;

                    foreach (var u in orgUsers)
                    {
                        userDict[u.Id] = u;
                        userOrgInfoDict[u.Id] = (orgName, collectorRate);
                    }
                }
            }
            else
            {
                var users = await _userRepository.GetFromOrganizationIdAsync(user.OrganizationId);
                userDict = users.ToDictionary(u => u.Id, u => u);

                var organization = await _organizationRepository.GetOrganizationById(user.OrganizationId);
                var orgName = organization?.OrganizationName ?? "Unknown Organization";
                var collectorRate = organization?.CollectorRate;

                foreach (var u in users)
                {
                    userOrgInfoDict[u.Id] = (orgName, collectorRate);
                }
            }

            var userIds = userDict.Keys.ToList();
            var collectorTransactions = await _transactionRepository.GetCollectorTransactionsByUserIdsAsync(userIds);

            var machineIds = collectorTransactions
                .Where(ct => !string.IsNullOrEmpty(ct.MachineId))
                .Select(ct => ct.MachineId!)
                .Distinct()
                .ToList();

            var machineDict = new Dictionary<string, Machine>();
            foreach (var machineId in machineIds)
            {
                var machine = await _machineRepository.GetByMachineIdAsync(machineId);
                if (machine != null)
                {
                    machineDict[machineId] = machine;
                }
            }

            var history = collectorTransactions
                .Where(ct => !string.IsNullOrEmpty(ct.MachineId)) // Only include completed collections
                .Select(ct =>
                {
                    Machine? machine = null;
                    machineDict?.TryGetValue(ct.MachineId ?? "", out machine);

                    User? collector = null;
                    userDict?.TryGetValue(ct.UserId, out collector);

                    var (organizationName, collectorRate) = userOrgInfoDict.TryGetValue(ct.UserId, out var orgInfo)
                        ? orgInfo
                        : ("Unknown Organization", (double?)null);

                    return new CollectorHistoryDto
                    {
                        DateTime = ct.CreatedAt,
                        MachineName = machine?.Location?.Name ?? "Unknown Location",
                        MachineId = ct.MachineId ?? "N/A",
                        SystemRecorded = ct.OilCollected,
                        CollectorRecorded = ct.CollectorRecordedOil,
                        CollectorName = collector?.Name ?? "Unknown Collector",
                        CollectorOrganization = organizationName,
                        CollectorRate = collectorRate
                    };
                })
                .OrderByDescending(h => h.DateTime)
                .ToList();

            return history;
        }
    }
}
