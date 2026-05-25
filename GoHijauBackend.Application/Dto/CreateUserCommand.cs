namespace GoHijauBackend.Application.Dto
{
    public class CreateUserCommand
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string NricOrPassport { get; set; } = null!;
        public string OrganizationId { get; set; } = null!;
        public HashSet<int> RoleId { get; set; } = null!;
    }
}
