using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IInvoicePdfService _invoicePdfService;
        private readonly IEmailService _emailService;
        public InvoiceService(IInvoiceRepository invoiceRepository, IUserRepository userRepository, IOrganizationRepository organizationRepository, ITransactionRepository transactionRepository,
             IInvoicePdfService invoicePdfService, IEmailService emailService)
        {
            _invoiceRepository = invoiceRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _transactionRepository = transactionRepository;
            _invoicePdfService = invoicePdfService;
            _emailService = emailService;
        }
        public async Task<Result<Invoice>> CreateInvoice(Invoice invoice, string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) {
                return Result.Failure<Invoice>("User doesn't exist");
            }
            if (user.OrganizationId == null || string.IsNullOrEmpty(user.OrganizationId))
            {
                return Result.Failure<Invoice>("Organization doesn't exist");
            }
            invoice.OrganizationId = user.OrganizationId;
            var organization = await _organizationRepository.GetOrganizationById(user.OrganizationId);
            double? rate =0;
            if (organization != null) { 
                rate = organization.CollectorRate;
            }
            if (!rate.HasValue || rate <= 0) {
                return Result.Failure<Invoice>("Collector rate is not set"); 
            }
            if (rate.HasValue) { 
                invoice.CollectorRate = rate.Value;
            }
            invoice.TotalAmount = invoice.OilCollected * invoice.CollectorRate;
            invoice.CreatedBy = userId;
            string nextInvoiceId = await _invoiceRepository.GetNextInvoiceId();
            invoice.InvoiceId = nextInvoiceId;
            var result = await _invoiceRepository.AddAsync(invoice);
            if (result == null) {
                return Result.Failure<Invoice>("Invoice Failed to create"); 
            }
            return Result.Success(invoice);
        }

        public async Task<Result> GetInvoiceForAllTransaction(string organizationId)
        {
 
            var organization = await _organizationRepository.GetOrganizationById(organizationId);
            double? rate = 0;
            if (organization != null)
            {
                rate = organization.CollectorRate;
            }
            else {
                return Result.Failure<Invoice>("Organization not found");
            }
            if (!rate.HasValue || rate <= 0)
            {
                return Result.Failure<Invoice>("Collector rate is not set");
            }
            var users = await _userRepository.GetFromOrganizationIdAsync(organizationId);
            var userIds = users.Select(u => u.Id).ToList();
            var collectorTransaction = await _transactionRepository.GetCollectorTransactionsByUserIdsAsync(userIds);

            if (collectorTransaction != null) 
            {
                foreach (var transaction in collectorTransaction) 
                {
                    string nextInvoiceId = await _invoiceRepository.GetNextInvoiceId();
                    var newInvoice = new Invoice
                    {
                        MachineId = transaction.MachineId,
                        OilCollected = transaction.OilCollected,
                        OrganizationId = organizationId,
                        InvoiceId = nextInvoiceId,
                        CollectorRate = rate.Value,
                        TotalAmount = transaction.OilCollected * rate.Value,
                    };
                    var result = await _invoiceRepository.AddAsync(newInvoice);
                    if (result == null)
                    {
                        return Result.Failure<Invoice>("Invoice Failed to create");
                    }
                    else 
                    {
                        var pdfBytes = _invoicePdfService.GenerateInvoicePdf(newInvoice, organization);
                        newInvoice.PdfPath = await _invoicePdfService.SaveInvoicePdfAsync(newInvoice, pdfBytes);

                        foreach (var email in organization.InvoiceEmails)
                        {
                            await _emailService.BuildAndSendInvoiceEmail(email, organization.OrganizationName, newInvoice, pdfBytes);
                        }
                    }
                   
                }
                return Result.Success("Invoices have been sent");
            }
            return Result.Success("No collector transactions found for this organization id");
        }
    }
}
