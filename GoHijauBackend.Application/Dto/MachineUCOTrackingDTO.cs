using System.ComponentModel.DataAnnotations;

namespace GoHijauBackend.Application.Dto
{
    public class MachineUCOTrackingDTO
    {
        [Required]
        public required string Id { get; set; }
        [Required]
        public required string MachineId { get; set; }
        [Required]
        public required string MachineLocationName { get; set; }
        [Required]
        public required double BufferVolume { get; set; } = 0;
        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; }
    }
}
