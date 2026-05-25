namespace GoHijauBackend.Application.Dto
{
    public class UpdateProfileDto
    {
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? NricOrPassport { get; set; }
        public string? OrganizationId { get; set; }
    }
}
