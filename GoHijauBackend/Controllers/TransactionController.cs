using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        private readonly IUserService _userService;

        public TransactionController(TransactionService transactionService, IUserService userService)
        {
            _transactionService = transactionService;
            _userService = userService;
        }

        [HttpGet("admin/total-oil")]
        public async Task<IActionResult> GetAdminTotalOil()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var user = await _userService.GetUserById(userId);
            var role = user?.Roles.FirstOrDefault();
            var userRole = ((UserRole)role).ToString();
            if (userRole == UserRole.GoHijauOwner.ToString() || userRole == UserRole.Admin.ToString()) {

                var total = await _transactionService.GetAdminTotalOilPoured();
                if (total == 0) return NotFound();

                return Ok(total);
        }
            else return Unauthorized();
        }

        [Authorize(Roles = "Owner")]
        [HttpGet("owner/total-oil")]
        public async Task<IActionResult> GetOwnerTotalOil()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
       
                var total = await _transactionService.GetOwnerTotalOilPoured(userId);
                if (total == 0) return NotFound();

                return Ok(total);
        }

        [HttpGet("user/all-transactions")]
        public async Task<IActionResult> AllTransactions()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var user = await _userService.GetUserById(userId);
            var role = user?.Roles.FirstOrDefault();
            var userRole = ((UserRole)role).ToString();

            if (userRole == UserRole.Customer.ToString()) {
                var transactionsList = await _transactionService.GetTransactionsByUserId(userId);
                if (transactionsList == null) return NotFound();

                return Ok(transactionsList);
            }
            if (userRole == UserRole.OilCollector.ToString()) {
                var transactionsList = await _transactionService.GetCollectorTransactionsByUserId(userId);
                if (transactionsList == null) return NotFound();

                return Ok(transactionsList);
            }
            return BadRequest();
        }

        [HttpGet("user/total-transaction")]
        public async Task<IActionResult> TotalTransaction()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var transaction = await _transactionService.GetTotalTransactionByUserId(userId);
            if (transaction == null) return NotFound();

            return Ok(transaction);
        }

        [HttpGet("owner/total-transaction")]
        public async Task<IActionResult> TotalOwnerTransaction()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var transaction = await _transactionService.GetTotalMachineOwnerTransactionByOrganizationId(userId);
            if (transaction == null) return NotFound();

            return Ok(transaction);
        }
    }
}
