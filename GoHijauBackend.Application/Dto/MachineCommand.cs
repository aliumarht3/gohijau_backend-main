using GoHijauBackend.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace GoHijauBackend.Application.Dto
{
    public class MachineCommand
    {
        [Required]
        public string MachineId { get; set; } = string.Empty;

        [Required]
        public LocationDto Location { get; set; } = new();

        [Required]
        public MachineType Type { get; set; }

        [Required]
        public DateTime ManufacturedDate { get; set; }

        [Required]
        public MachineStatus Status { get; set; }

        [Required]
        public string Owner { get; set; } = string.Empty;

        [Required]
        public string Collector { get; set; } = string.Empty;

        [Required]
        public string Technician { get; set; } = string.Empty;
    }
}
