using GoHijauBackend.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace GoHijauBackend.Domain.Entities
{
    public class MachineOwnerLogCommand
    {
        [Required]
        public string CustomerTransactionId { get; set; }

        [Required]
        public string MachineOwnerOrganizationId { get; set; }

        [Required]
        public string MachineId { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [Required]
        public double UcoWeight { get; set; }

        public double ProfitRate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public MachineOwnerProfitLogType Type { get; set; }

        [Required]
        public decimal NewEwalletBalance { get; set; }

        [Required]
        public decimal NewDebtBalance { get; set; }
    }
}
