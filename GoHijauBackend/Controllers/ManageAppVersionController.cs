using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ManageAppVersionController : ControllerBase
    {

        [HttpGet("get-latest-app-version")]
        public async Task<IActionResult> GetAdminTotalOil()
        {
            var response = new
            {
                latestVersion = "3.0.14",
                latestBuildNumber = new
                {
                    android = 28,
                    ios = 18
                },
                updateUrl = new
                {
                    android = "https://play.google.com/store/apps/details?id=com.myro.gohijau&hl=en",
                    ios = "https://apps.apple.com/my/app/gohijau/id1639067146?l=ms"
                }
            };

            return Ok(response);
        }

    }
}
