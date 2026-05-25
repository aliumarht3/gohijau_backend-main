using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Interfaces.Services.ActivityLogs;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System.Net.Mail;

namespace GoHijauBackend.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IHostEnvironment _env;
        private readonly IUserRepository _userRepository;
        private readonly IMachineOwnerProfitDebtLogService _machineOwnerProfitDebtLogService;
        private readonly IMongoClient _mongoClient;

        public OrganizationService(
            IOrganizationRepository organizationRepository, 
            IHostEnvironment env, 
            IUserRepository userRepository, 
            IMachineOwnerProfitDebtLogService machineOwnerProfitDebtLogService,
            IMongoClient mongoClient
            )
        {
            _organizationRepository = organizationRepository;
            _env = env;
            _userRepository = userRepository;
            _machineOwnerProfitDebtLogService = machineOwnerProfitDebtLogService;
            _mongoClient = mongoClient;
        }

        public async Task<Result> CreateOrganization(string userId, OrganizationCommand organizationCommand)
        {
            try
            {
                var file = organizationCommand.CompanyCertificate;
                if (file == null || file.Length == 0)
                    return Result.Failure("Certificate file is required.");

                var relativePath = await SaveCertificate(file);
                var organizationAddress = new Address(
                    organizationCommand.UnitNo,
                    organizationCommand.Street,
                    organizationCommand.District,
                    organizationCommand.Postcode,
                    organizationCommand.State,
                    organizationCommand.Country
                );

                // Create entity
                var organization = new Organization
                {
                    OrganizationName = organizationCommand.OrganizationName,
                    Address = organizationAddress,
                    OrganizationTypes = organizationCommand.OrganizationTypes ?? new List<OrganizationType>(),
                    CertificatePath = relativePath,
                    CollectorRate = organizationCommand.CollectorRate,
                    ProfitRate = organizationCommand.ProfitRate,
                    CustomerRate = organizationCommand.CustomerRate,
                    CreditLimit = organizationCommand.Creditlimit,
                    TotalDebtAssigned = organizationCommand.Debt,
                    OutstandingDebt = organizationCommand.Debt,
                    InvoiceEmails = organizationCommand.InvoiceEmails?
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Select(e => e.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList() ?? new List<string>(),
                    NotificationEmails = organizationCommand.NotificationEmails?
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Select(e => e.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList() ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _organizationRepository.AddAsync(organization);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to create organization: {ex.Message}");
            }
        }

        public async Task<Organization?> GetOrganizationById(string organizationId)
        {
            return await _organizationRepository.GetOrganizationById(organizationId);
        }

        public async Task<List<Organization>> GetAllOrganizations()
        {
            return await _organizationRepository.GetAllOrganizationsAsync();
        }

        public async Task<List<Organization>> GetAllOilCollectorsOrganizations()
        {
            return await GetOrganizationsByRole(UserRole.OilCollector);
        }

        public async Task<List<Organization>> GetAllOwnersOrganizations()
        {
            return await GetOrganizationsByRole(UserRole.Owner);
        }

        public async Task<List<Organization>> GetAllTechniciansOrganizations()
        {
            return await GetOrganizationsByRole(UserRole.Technician);
        }

        private async Task<string> SaveCertificate(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "organizations", "certificates");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/organizations/certificates/{uniqueFileName}";
        }

        private async Task<List<Organization>> GetOrganizationsByRole(UserRole role)
        {
            var organizationIds = await _userRepository.GetDistinctOrganizationIdsByRole(role);

            if (!organizationIds.Any())
                return new List<Organization>();

            var organizations = await _organizationRepository.GetOrganizationsByIds(organizationIds);

            return organizations;
        }

        public async Task<Result> UpdateOrganization(string userId, string organizationId, OrganizationCommand command, IClientSessionHandle? session = null)
        {
            try
            {
                var existing = await _organizationRepository.GetOrganizationById(organizationId);
                if (existing == null)
                    return Result.Failure("Organization not found.");

                string certificatePath = existing.CertificatePath;
                if (command.CompanyCertificate != null && command.CompanyCertificate.Length > 0)
                {
                    certificatePath = await SaveCertificate(command.CompanyCertificate);
                }

                var updatedAddress = new Address(
                    command.UnitNo,
                    command.Street,
                    command.District,
                    command.Postcode,
                    command.State,
                    command.Country
                );

                existing.OrganizationName = command.OrganizationName;
                existing.Address = updatedAddress;
                existing.OrganizationTypes = command.OrganizationTypes ?? existing.OrganizationTypes;
                existing.CertificatePath = certificatePath;
                existing.CollectorRate = command.CollectorRate;
                existing.ProfitRate = command.ProfitRate;
                existing.CustomerRate = command.CustomerRate;
                existing.CreditLimit = command.Creditlimit;
                existing.InvoiceEmails = command.InvoiceEmails?
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? existing.InvoiceEmails;
                existing.NotificationEmails = command.NotificationEmails?
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? existing.NotificationEmails;
                existing.ModifiedAt = DateTime.UtcNow;
                existing.ModifiedBy = userId;

                await _organizationRepository.UpdateOrganizationAsync(organizationId, existing, session);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update organization: {ex.Message}");
            }
        }

        public async Task<Result> AddOrganizationDebt(
            string userId,
            string organizationId,
            decimal debt
        )
        {
            IClientSessionHandle session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var organization = await _organizationRepository.GetOrganizationById(organizationId);
                if (organization == null)
                    return Result.Failure("Organization not found.");
                var newOutstandingDebt = (decimal)(organization?.OutstandingDebt ?? 0) + debt;

                var logResult = await _machineOwnerProfitDebtLogService.AddDebtLog
                    (userId,
                    new MachineOwnerLogCommand
                    {
                        CustomerTransactionId = null,
                        MachineOwnerOrganizationId = organizationId,
                        MachineId = string.Empty,
                        CustomerId = userId,
                        UcoWeight = 0.0,
                        ProfitRate = organization.CollectorRate ?? 0.0,
                        Amount = debt,
                        NewEwalletBalance = 0m,
                        NewDebtBalance = newOutstandingDebt
                    },
                    session);

                if (!logResult.IsSuccess)
                    return Result.Failure(logResult.Error ?? "Failed to write debt log.");

                var updated = await _organizationRepository.AddOrganizationTotalNOutstandingDebt(
                    organizationId,
                    Convert.ToDouble(debt),
                    userId,
                    DateTime.UtcNow,
                    session
                );

                if (!updated)
                    return Result.Failure("Organization not found.");

                await session.CommitTransactionAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                return Result.Failure($"Failed to update debt: {ex.Message}");
            }
        }

        public async Task<Result> UpdateOrganizationOutstandingDebt(
            string userId,
            string organizationId,
            decimal debt,
            IClientSessionHandle? session = null
        )
        {
            try
            {
                var updated = await _organizationRepository.UpdateOrganizationOutstandingDebt(
                    organizationId,
                    Convert.ToDouble(debt),
                    userId,
                    DateTime.UtcNow,
                    session
                );

                if (!updated)
                    return Result.Failure("Organization not found.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update debt: {ex.Message}");
            }
        }

        public async Task<Result> DeleteOrganization(string organizationId)
        {
            try
            {
                var existing = await _organizationRepository.GetOrganizationById(organizationId);
                if (existing == null)
                    return Result.Failure("Organization not found.");

                await _organizationRepository.DeleteOrganizationAsync(organizationId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to delete organization: {ex.Message}");
            }
        }

        public async Task<Result> UpdateMachineOwnerRate(string userId, double machineOwnerRate, string organizationId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Failure("UserId is required.");

            if (string.IsNullOrWhiteSpace(organizationId))
                return Result.Failure("OrganizationId is required.");

            try
            {
                await _organizationRepository.UpdateMachineOwnerRate(
                    machineOwnerRate,
                    organizationId,
                    userId
                );
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update machine owner rate: {ex.Message}");
            }
        }

        public async Task<Result> UpdateCollectorRate(string userId, double collectorRate, string organizationId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Failure("UserId is required.");

            if (string.IsNullOrWhiteSpace(organizationId))
                return Result.Failure("OrganizationId is required.");

            try
            {
                await _organizationRepository.UpdateCollectorRate(
                    collectorRate,
                    organizationId,
                    userId
                );
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update machine owner rate: {ex.Message}");
            }
        }

        public async Task<Result> AddOrganizationEmails(string userId, IEnumerable<string>? invoiceEmails, IEnumerable<string>? notificationEmails, string organizationId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result.Failure("UserId is required.");

            if (string.IsNullOrWhiteSpace(organizationId))
                return Result.Failure("OrganizationId is required.");

            var sanitizedInvoiceEmails = (invoiceEmails ?? Enumerable.Empty<string>())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sanitizedNotificationEmails = (notificationEmails ?? Enumerable.Empty<string>())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!sanitizedInvoiceEmails.Any() && !sanitizedNotificationEmails.Any())
                return Result.Failure("At least one valid email is required.");

            // Basic email validation
            foreach (var email in sanitizedInvoiceEmails.Concat(sanitizedNotificationEmails))
            {
                try
                {
                    var addr = new MailAddress(email);
                    if (string.IsNullOrWhiteSpace(addr.Address))
                        return Result.Failure($"Invalid email: {email}");
                }
                catch
                {
                    return Result.Failure($"Invalid email: {email}");
                }
            }

            try
            {
                await _organizationRepository.AddOrganizationEmails(sanitizedInvoiceEmails, sanitizedNotificationEmails, organizationId, userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to add organization emails: {ex.Message}");
            }
        }
    }
}
