using GoHijauBackend.Domain.Entities;


namespace GoHijauBackend.Application.Interfaces.Persistence
{
    public interface IInvoiceRepository
    {
        Task <Invoice> AddAsync(Invoice invoice);
        Task<string> GetNextInvoiceId();
    }
}
