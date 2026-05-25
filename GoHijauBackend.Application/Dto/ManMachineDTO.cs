using GoHijauBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Dto
{
    public class ManMachineDTO
    {
        [Required]
        public string MachineId { get; set; } = string.Empty;

        [Required]
        public ManMachineStatus Status { get; set; }
    }
}
