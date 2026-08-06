using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using QuestPDF.Fluent;


namespace GoHijauBackend.Application.Services
{
    public class InvoicePdfService : IInvoicePdfService
    {
        public byte[] GenerateInvoicePdf(Invoice invoice, Organization organization)
        {
            var document = new InvoiceDocument(invoice, organization);
            return document.GeneratePdf();
        }

        public async Task<string> SaveInvoicePdfAsync(Invoice invoice, byte[] pdfBytes)
        {
            var folder = Path.Combine("storage", "invoices");
            Directory.CreateDirectory(folder);

            var fileName = $"{invoice.InvoiceId}.pdf";
            var path = Path.Combine(folder, fileName);

            await File.WriteAllBytesAsync(path, pdfBytes);
            return path;
        }
    }
}
