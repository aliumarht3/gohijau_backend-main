using GoHijauBackend.Domain.Entities;

namespace GoHijauBackend.Application.Dto
{
    public class MachineSearchRequest
    {
        public string? Name { get; set; }
        public string? UnitNo { get; set; }
        public string? Street { get; set; }
        public string? District { get; set; }
        public string? Postcode { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Coordinate { get; set; }
        public MachineType? Type { get; set; }
        public MachineStatus? Status { get; set; }
        public string? Owner { get; set; }
        public DateTime? ManufacturedDateFrom { get; set; }
        public DateTime? ManufacturedDateTo { get; set; }
    }
}
