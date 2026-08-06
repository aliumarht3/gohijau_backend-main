using System.ComponentModel.DataAnnotations;

namespace GoHijauBackend.Application.Dto
{
    public class LocationDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

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

        [Required]
        public string Coordinates { get; set; } = string.Empty;
    }
}
