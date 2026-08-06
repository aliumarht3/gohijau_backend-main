using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;

namespace GoHijauBackend.Application.Services
{
    public class MachineHistoryService : IMachineHistoryService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMachineRepository _machineRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public MachineHistoryService(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            IMachineRepository machineRepository,
            IOrganizationRepository organizationRepository)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _machineRepository = machineRepository;
            _organizationRepository = organizationRepository;
        }

        public async Task<List<DepositionHistoryDto>> GetDepositionHistoryByMachineIdAsync(string machineId)
        {

            var transactions = await _transactionRepository.GetByMachineIdAsync(machineId);

            if (transactions == null || transactions.Count == 0)
                return new List<DepositionHistoryDto>();

 
            var machine = await _machineRepository.GetByMachineIdAsync(machineId);
            

            double profitRate = 0.9; 
            if (machine != null && !string.IsNullOrEmpty(machine.Owner))
            {
                var organization = await _organizationRepository.GetOrganizationById(machine.Owner);
                if (organization?.ProfitRate != null && organization.ProfitRate > 0)
                {
                    profitRate = organization.ProfitRate.Value;
                }
            }

            var depositionHistory = new List<DepositionHistoryDto>();

            foreach (var transaction in transactions)
            {
                var user = await _userRepository.GetByIdAsync(transaction.UserId);
                var userName = user?.Name ?? "Unknown User";

                depositionHistory.Add(new DepositionHistoryDto
                {
                    DateTime = transaction.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Amount = transaction.OilPoured.ToString("F1"),
                    User = userName,
                    CustomerReward = transaction.PointsAwarded,
                    OwnerReward = transaction.OilPoured * profitRate
                });
            }

            return depositionHistory;
        }

        public async Task<List<CollectionHistoryDto>> GetCollectionHistoryByMachineIdAsync(string machineId)
        {

            var collectorTransactions = await _transactionRepository.GetCollectorTransactionsByMachineIdAsync(machineId);

            if (collectorTransactions == null || collectorTransactions.Count == 0)
                return new List<CollectionHistoryDto>();

            var collectionHistory = new List<CollectionHistoryDto>();

            foreach (var transaction in collectorTransactions)
            {

                var collector = await _userRepository.GetByIdAsync(transaction.UserId);
                var collectorName = collector?.Name ?? "Unknown Collector";

                collectionHistory.Add(new CollectionHistoryDto
                {
                    DateTime = transaction.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Collector = collectorName,
                    GohijauRecorded = transaction.OilCollected.ToString("F1"),
                    CollectorRecorded = transaction.CollectorRecordedOil.ToString("F1")
                });
            }

            return collectionHistory;
        }
    }
}
