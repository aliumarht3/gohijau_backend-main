using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Dto;
using GoHijauBackend.Domain.Entities;


namespace GoHijauBackend.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task<Result> SendEmail(EmailDto emailDto);
        Task<Result> BuildAndSendReminderEmail(string email, string organizationName, string machineName, string machineLocation, int capacityPercentage, DateTime sentAtLocal);
        Task<Result> BuildAndSendNewAccountEmail(string recipientEmail, string tempPassword);
        Task<Result> BuildAndSendInvoiceEmail(string email, string customerName, Invoice invoice, byte[]? pdfBytes);
        Task<Result> BuildAndSendPassswordResetEmail(string recipientEmail, string primaryLink, string? deepLink = null);
    }
}
