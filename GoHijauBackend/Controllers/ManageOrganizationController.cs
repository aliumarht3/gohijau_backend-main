using GoHijauBackend.Application.Dto;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/organization")]
    public class ManageOrganizationController : ControllerBase
    {
        private readonly IOrganizationService _service;
        public ManageOrganizationController(IOrganizationService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin,GoHijauOwner")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrganization([FromForm] OrganizationCommand organizationCommand)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.CreateOrganization(userId, organizationCommand);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Organization Created Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-machine-owner-rate")]
        public async Task<IActionResult> UpdateMachineOwnerRate([FromForm] double machineOwnerRate, [FromForm] string organizationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (machineOwnerRate == null)
                return BadRequest(new { message = "Machine owner rate is required." });

            if (organizationId == null)
                return BadRequest(new { message = "Organization Id is required." });

            // Optional business validation
            if (double.IsNaN(machineOwnerRate) || double.IsInfinity(machineOwnerRate))
                return BadRequest(new { message = "Invalid machine owner rate." });

            if (machineOwnerRate < 0)
                return BadRequest(new { message = "Machine owner rate must be more than 0." });

            try
            {
                var result = await _service.UpdateMachineOwnerRate(userId, machineOwnerRate, organizationId);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Machine Owner Rate Updated Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update-collector-rate")]
        public async Task<IActionResult> UpdateCollectorRate([FromForm] double collectorRate, [FromForm] string organizationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (collectorRate == null)
                return BadRequest(new { message = "Collector rate is required." });

            if (organizationId == null)
                return BadRequest(new { message = "Organization Id is required." });

            // Optional business validation
            if (double.IsNaN(collectorRate) || double.IsInfinity(collectorRate))
                return BadRequest(new { message = "Invalid collector rate." });

            if (collectorRate < 0)
                return BadRequest(new { message = "Collector rate must be more than 0." });

            try
            {
                var result = await _service.UpdateCollectorRate(userId, collectorRate, organizationId);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Collector Rate Updated Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-email")]
        public async Task<IActionResult> AddOrganizationEmails([FromBody] AddOrganizationEmailsRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(request.OrganizationId))
                return BadRequest(new { message = "OrganizationId is required." });

            var hasInvoice = request.InvoiceEmails != null && request.InvoiceEmails.Any();
            var hasNotification = request.NotificationEmails != null && request.NotificationEmails.Any();

            if (!hasInvoice && !hasNotification)
                return BadRequest(new { message = "At least one of InvoiceEmails or NotificationEmails is required." });

            try
            {
                var result = await _service.AddOrganizationEmails(userId, request.InvoiceEmails, request.NotificationEmails, request.OrganizationId);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Successfully Added Organization Email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,GoHijauOwner")]
        [HttpPost("add-debt")]
        public async Task<IActionResult> AddOrganizationDebt([FromForm] decimal debt, [FromForm] string organizationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(organizationId))
                return BadRequest(new { message = "OrganizationId is required." });

            // Basic validation
            if (debt < 0)
                return BadRequest(new { message = "Debt must be non-negative." });

            try
            {
                var result = await _service.AddOrganizationDebt(userId, organizationId, debt);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Organization debt updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,GoHijauOwner")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var organization = await _service.GetOrganizationById(id);
                if (organization == null)
                    return NotFound(new { message = "Organization not found." });

                return Ok(organization);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,GoHijauOwner,Owner,OilCollector")]
        [HttpPatch("update/{id}")]
        public async Task<IActionResult> UpdateOrganization(string id, [FromForm] OrganizationCommand command)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _service.UpdateOrganization(userId, id, command);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Organization Updated Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,GoHijauOwner")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteOrganization(string id)
        {
            try
            {
                var result = await _service.DeleteOrganization(id);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { message = "Organization Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var organizations = await _service.GetAllOrganizations();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("oilCollectors")]
        public async Task<IActionResult> GetOilCollectors()
        {
            try
            {
                var organizations = await _service.GetAllOilCollectorsOrganizations();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("owners")]
        public async Task<IActionResult> GetOwners()
        {
            try
            {
                var organizations = await _service.GetAllOwnersOrganizations();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("technicians")]
        public async Task<IActionResult> GetTechnicians()
        {
            try
            {
                var organizations = await _service.GetAllTechniciansOrganizations();

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
