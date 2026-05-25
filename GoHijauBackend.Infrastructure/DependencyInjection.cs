using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Persistence.External;
using GoHijauBackend.Application.Interfaces.Persistence.ActivityLogs;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Services;
using GoHijauBackend.Domain.Entities.External.Clients;
using GoHijauBackend.Infrastructure.Auth;
using GoHijauBackend.Infrastructure.External;
using GoHijauBackend.Infrastructure.Persistence;
using GoHijauBackend.Infrastructure.Persistence.ActivityLogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace GoHijauBackend.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, MongoUserRepository> ();
            services.AddScoped<IRolesRepository, MongoRolesRepository>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IManageUserRoleService, ManageUserRoleService>();
            services.AddScoped<IQrTokenRepository, MongoQrTokenRepository>();
            services.AddScoped<ITotalTransactionRepository, MongoTotalTransactionRepository>();
            services.AddScoped<ITransactionRepository, MongoTransactionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IMachineRepository, MongoMachineRepository>();
            services.AddScoped<IMachineAuditRepository, MongoMachineAuditRepository>();
            services.AddScoped<IManMachineRepository, MongoManMachineRepository>();
            services.AddScoped<IOrganizationRepository, MongoOrganizationRepository>();
            services.AddScoped<IRateRepository, MongoRateRepository>();
            services.AddScoped<ICustomerBankAccountRepository, MongoCustomerBankAccountRepository>();
            services.AddScoped<IMachineOwnerBankAccountRepository, MongoMachineOwnerBankAccountRepository>();
            services.AddScoped<IMachineUCOTrackingRepository, MongoMachineUCOTrackingRepository>();
            services.AddScoped<ICustomerPayoutGateway, CustomerPayoutGateway>();
            services.AddScoped<ICollectorBillingRepository, MongoCollectorBillingRepository>();
            services.AddScoped<PayoutApiClient>();
            services.AddScoped<IWithdrawalHistoryRepository, MongoWithdrawalHistoryRepository>();
            services.AddScoped<IRazorPayRepository, MongoRazorPayRepository>();
            services.AddScoped<ISecretRepository, MongoSecretRepository>();
            services.AddScoped<IMachineOwnerPayoutRepository, MongoMachineOwnerPayoutRepository>();
            services.AddScoped<IDashboardAnalyticsRepository, MongoDashboardAnalyticsRepository>(); 
            services.AddScoped<IInvoiceRepository, MongoInvoiceRepository>();
            services.AddScoped<ICollectorUCOReminderRepository, MongoCollectorUCOReminderRepository>();
            services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
            services.AddScoped<IMachineOwnerProfitDebtLogRepository, MachineOwnerProfitDebtLogRepository>();
            services.AddSingleton<IMongoClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration["MongoDB:ConnectionString"];
                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var dbName = configuration["MongoDB:Database"];
                return client.GetDatabase(dbName);
            });

            return services;
        }
    }
}
