using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/razorpay")]
    public class ManageRazorPayController : ControllerBase
    {
        private readonly IRazorPayService _service;
        private readonly ISecretService _secret; 
        public ManageRazorPayController( IRazorPayService service, ISecretService secret) { _service = service; _secret = secret;}

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] RazorOrderDTO order)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();
            if (order == null || order.Amount <= 0)
                return BadRequest(new { message = "Invalid amount" });
            try
            {
                var result = await  _service.CreateOrder(order.Amount, userId);
                var response = new
                {
                    id = result["id"].ToString(),
                    amount = result["amount"],
                    currency = result["currency"],
                    receipt = result["receipt"]
                };
                if (result==null)
                    return BadRequest(new { message = "Order creation failed"});

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
        [HttpPost("callback")]
        public async Task<IActionResult> RazorpayCallback([FromForm] RazorpayCallbackDto model)
        {
            //migrate into IRazorPayService later
            var keySecret = _secret.GetSecret(); 
            var order = _service.GetOrder(model.razorpay_order_id); 
            var body = order.Result.OrderId+ "|" + model.razorpay_payment_id;
            var expectedSignature = CreateHmacSha256(body, keySecret.Result.Value.KeySecret);

            if (!string.Equals(expectedSignature, model.razorpay_signature, StringComparison.OrdinalIgnoreCase))
            {
                //live url 
                return Redirect("https://dashboard.gohijau.org/machineCollector");
                //development redirect url 
                //return Redirect("http://localhost:5173/machineCollector");
            }

             await _service.MarkPaymentAsPaid(model.razorpay_order_id,model.razorpay_payment_id);

            //live url 
            return Redirect("https://dashboard.gohijau.org/machineCollector");
            //development redirect url 
            //return Redirect("http://localhost:5173/machineCollector");
        }

        private static string CreateHmacSha256(string message, string secret)
        {
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(secret);
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

            using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

    }
}
