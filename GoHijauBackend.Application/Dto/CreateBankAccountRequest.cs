namespace GoHijauBackend.Application.Dto
{
    public class CreateBankAccountRequest
    {
        public string BankCode { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
    }
}
