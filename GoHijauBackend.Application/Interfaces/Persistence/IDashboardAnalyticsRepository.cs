namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IDashboardAnalyticsRepository
    {
        public Task<double> GetOwnerTotalIncome(string userId , double totalUCO);
        public Task<int> GetOwnerTotalActiveMachines(string userId);

    }
}
