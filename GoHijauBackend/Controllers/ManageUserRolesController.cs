using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api")]
    public class ManageUserRolesController : ControllerBase
    {
        private readonly IManageUserRoleService _manageUserRoleService;
        private readonly ILogger<ManageUserRolesController> _logger;
        public ManageUserRolesController(IManageUserRoleService manageUserRoleService, ILogger<ManageUserRolesController> logger)
        {
            _manageUserRoleService = manageUserRoleService;
            _logger = logger;
        }

        [HttpPost("add-role/{userId}")]
        public async Task<IActionResult> AddRole(string userId, [FromBody] int roleId)
        {
            try
            {
                _logger.LogInformation("add role: {role}", roleId);
                await _manageUserRoleService.Execute(userId, roleId);
                return Ok(new { message = "Role added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("roles/all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _manageUserRoleService.GetAllRoles();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
