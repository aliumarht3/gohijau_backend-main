using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Persistence;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GoHijauBackend.Application.Services
{
    public class CollectionPostProcessService : ICollectionPostProcessService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IInvoicePdfService _invoicePdfService;
        private readonly IEmailService _emailService;
        private readonly ILogger<CollectionPostProcessService> _logger;

        public CollectionPostProcessService(
            IInvoiceService invoiceService,
            IOrganizationRepository organizationRepository,
            IInvoicePdfService invoicePdfService,
            IEmailService emailService,
            ILogger<CollectionPostProcessService> logger)
        {
            _invoiceService = invoiceService;
            _organizationRepository = organizationRepository;
            _invoicePdfService = invoicePdfService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ProcessAsync(CollectionPostProcessJob job, CancellationToken cancellationToken = default)
        {
            var newInvoice = new Invoice
            {
                MachineId = job.MachineId,
                OilCollected = job.OilCollected
            };

            var invoiceResult = await _invoiceService.CreateInvoice(newInvoice, job.UserId);
            if (!invoiceResult.IsSuccess)
            {
                throw new InvalidOperationException($"Invoice creation failed: {invoiceResult.Error}");
            }

            var invoice = invoiceResult.Value;
            var organization = await _organizationRepository.GetOrganizationById(invoice.OrganizationId);

            if (organization == null)
            {
                throw new InvalidOperationException($"Organization not found for invoice: {invoice.OrganizationId}");
            }

            var pdfBytes = _invoicePdfService.GenerateInvoicePdf(invoice, organization);
            invoice.PdfPath = await _invoicePdfService.SaveInvoicePdfAsync(invoice, pdfBytes);

            if (organization.InvoiceEmails == null || organization.InvoiceEmails.Count == 0)
            {
                _logger.LogError($"No invoice recipients configured for organization: {organization.Id}");
            }

            foreach (var email in organization.InvoiceEmails)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sendResult = await _emailService.BuildAndSendInvoiceEmail(email, organization.OrganizationName, invoice, pdfBytes);
                if (sendResult.IsFailure)
                {
                    _logger.LogError($"Invoice email send failed for recipient '{email}' and invoice '{invoice.InvoiceId}': {sendResult.Error}");
                }
            }
        }
    }
}
