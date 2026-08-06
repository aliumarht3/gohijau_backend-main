using System.ComponentModel.DataAnnotations;

namespace GoHijauBackend.Application.Dto
{
    public class MachineAuditRequest
    {
        [Required]
        public string MachineId { get; set; } = string.Empty;

        [Required]
        public string QrToken { get; set; } = string.Empty;
        [Required]
        public string Action { get; set; } = string.Empty;
    }
}
