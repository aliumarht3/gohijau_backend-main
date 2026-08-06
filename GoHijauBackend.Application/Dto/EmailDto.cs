using System.ComponentModel.DataAnnotations;
namespace GoHijauBackend.Application.Dto
{
    public class EmailDto
    {
        [Required]
        public string EmailTo { get; set; } = string.Empty;

        [Required]
        public string EmailSubject { get; set; } = string.Empty;

        [Required]
        public string EmailContent { get; set; } = string.Empty;
        public byte[]? EmailAttachmentBytes { get; set; }
        public string EmailAttachmentName { get; set; } = string.Empty;    
    }
}
