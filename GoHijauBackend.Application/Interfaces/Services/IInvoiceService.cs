using CSharpFunctionalExtensions;
using GoHijauBackend.Domain.Entities;
namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<Result<Invoice>> CreateInvoice(Invoice invoice, string userId);
        Task<Result> GetInvoiceForAllTransaction(string organizationId);
    }
}
