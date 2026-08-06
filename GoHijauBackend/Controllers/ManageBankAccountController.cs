using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class ManageBankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;

        public ManageBankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        //[Authorize(Roles = "Customer")]
        [HttpPost("customer/create-bank-account")]
        [HttpPost("customer/create-or-update-bank-account")]
        public async Task<IActionResult> CreateOrUpdateBankAccount([FromBody] CreateBankAccountRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _bankAccountService.CreateOrUpdateBankAccount(userId, request);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Bank account linked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("customer/get-bank-account")]
        public async Task<IActionResult> GetBankAccount() 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            try
            {
                var result = await _bankAccountService.GetBankAccount(userId);
                if (result!= null) {
                    return Ok(new
                    {
                        bankCode = result.BankCode,
                        accountNumber = result.AccountNumber
                    });
                }
                return Ok(); 
             
            }
            catch (Exception ex) {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("machine-owner/create-or-update-bank-account/{userId}")]
        public async Task<IActionResult> CreateOrUpdateMachineOwnerBankAccount(string userId, [FromBody] CreateBankAccountRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _bankAccountService.CreateOrUpdateMachineOwnerBankAccount(userId, request);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Bank account linked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
