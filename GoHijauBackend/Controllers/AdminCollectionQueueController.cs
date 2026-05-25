using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/admin/collection-queue")]
    [Authorize(Roles = "Admin")]
    public class AdminCollectionQueueController : ControllerBase
    {
        private readonly ICollectionPostProcessQueue _queue;

        public AdminCollectionQueueController(ICollectionPostProcessQueue queue)
        {
            _queue = queue;
        }

        [HttpGet("dead")]
        public async Task<IActionResult> GetDeadJobs([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                var jobs = await _queue.GetDeadJobsAsync(limit, cancellationToken);
                return Ok(new { count = jobs.Count, jobs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("dead/{jobId}/replay")]
        public async Task<IActionResult> ReplayDeadJob(string jobId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return BadRequest(new { message = "jobId is required." });
            }

            try
            {
                var replayed = await _queue.ReplayDeadJobAsync(jobId, cancellationToken);
                if (!replayed)
                {
                    return NotFound(new { message = "Dead job not found for the provided id." });
                }

                return Ok(new { message = "Job has been re-queued." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
