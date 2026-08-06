using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    public class ManageInvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public ManageInvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("generate-all-invoice")]
        public async Task<IActionResult> GenerateInvoiceForAllTransactions(string organizationId)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (userId == null) return Unauthorized();

            try
            {
                var result = await _invoiceService.GetInvoiceForAllTransaction(organizationId);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Invoice added succesfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
