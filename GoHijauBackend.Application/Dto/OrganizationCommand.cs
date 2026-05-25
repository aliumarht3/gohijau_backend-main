using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GoHijauBackend.Application.Dto
{
    public class OrganizationCommand
    {
        [Required]
        public string OrganizationName { get; set; } = string.Empty;

        [Required]
        public string UnitNo { get; set; } = string.Empty;

        [Required]
        public string Street { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        [Required]
        public string Postcode { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public IFormFile? CompanyCertificate { get; set; }

        public List<OrganizationType>? OrganizationTypes { get; set; }

        public double? CollectorRate { get; set; }

        public double? ProfitRate { get; set; }

        public double? CustomerRate { get; set; }

        public double? Debt { get; set; }

        public double? Creditlimit { get; set; }

        public List<string>? InvoiceEmails { get; set; }

        public List<string>? NotificationEmails { get; set; }
    }
}
