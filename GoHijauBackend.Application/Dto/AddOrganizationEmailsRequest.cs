namespace GoHijauBackend.Application.Dto
{
    public class AddOrganizationEmailsRequest
    {
        public List<string> InvoiceEmails { get; set; } = new List<string>();
        public List<string> NotificationEmails { get; set; } = new List<string>();
        public string OrganizationId { get; set; } = string.Empty;
    }
}
