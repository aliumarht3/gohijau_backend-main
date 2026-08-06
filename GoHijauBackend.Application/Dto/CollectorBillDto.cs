namespace GoHijauBackend.Application.Dto
{
    public class CollectorBillDto
    {
        public decimal credit { get; set; } = 0; 
        public decimal currentBill { get; set; } = 0; 
        public decimal overDue { get; set; } = 0; 
    }
}
