using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin,GoHijauOwner")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var result = await _userService.CreateUser(userId, command);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "User created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("by-organization/{organizationId}")]
        public async Task<IActionResult> GetUsersByOrganization(string organizationId)
        {
            try
            {
                var users = await _userService.GetUserFromOrganizationId(organizationId);
                return Ok(users.Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.Name,
                    u.Phone,
                    u.NricOrPassport,
                    u.OrganizationId,
                    u.Roles
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _userService.GetUserById(userId);
            if (user == null) return NotFound();
            var role = user.Roles.FirstOrDefault();
            var userRole = ((UserRole)role).ToString();
            var allRoles = user.Roles.Select(r => ((UserRole)r).ToString()).ToList();
            return Ok(new
            {
                user.Id,
                user.Email,
                user.Name,
                user.Phone,
                user.OrganizationId,
                userRole,
                allRoles
            });
        }

        [HttpPatch("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            var roleClaims = User.FindAll(ClaimTypes.Role)
                     .Select(r => r.Value)
                     .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool isAdmin = roleClaims.Contains(UserRole.Admin.ToString());

            var targetUserId = isAdmin && !string.IsNullOrEmpty(dto.UserId)
                ? dto.UserId
                : currentUserId;

            try
            {
                var updatedUser = await _userService.UpdateUser(targetUserId, dto);
                if (updatedUser == null) return NotFound();

                return Ok(new
                {
                    updatedUser.Id,
                    updatedUser.Email,
                    updatedUser.Name,
                    updatedUser.Phone
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("restore/{targetUserEmail}")]
        public async Task<IActionResult> RestoreUser(string targetUserEmail)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            var roleClaims = User.FindAll(ClaimTypes.Role)
                     .Select(r => r.Value)
                     .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool isAdmin = roleClaims.Contains(UserRole.Admin.ToString());
            
            if (!isAdmin)
                return Forbid("Only admins can restore users.");

            if (string.IsNullOrEmpty(targetUserEmail))
                return BadRequest(new { message = "Deleted user email is required." });

            try
            {
                var restoredUserEmail = await _userService.RestoreUser(targetUserEmail, currentUserId);
                if (restoredUserEmail == null) return NotFound(new { message = "User not found or not deleted." });

                return Ok(new { message = $"User with {restoredUserEmail} is restored successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                await _userService.ChangePasswordAsync(userId, dto);
                return Ok(new { message = "Password Changed Successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            var roleClaims = User.FindAll(ClaimTypes.Role)
                     .Select(r => r.Value)
                     .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool isAdmin = roleClaims.Contains(UserRole.Admin.ToString());

            try
            {
                string resultMessage = await _userService.DeleteAccount(dto, currentUserId, isAdmin);
                return Ok(new { message = resultMessage });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
