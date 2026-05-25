using CSharpFunctionalExtensions;
using GoHijauBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    public class ManageCollectorUCOReminder : ControllerBase
    {
        private readonly ICollectorUCOReminder _collectorUCOReminder; 
        public ManageCollectorUCOReminder(ICollectorUCOReminder collectorUCOReminder) { 
            _collectorUCOReminder = collectorUCOReminder;
        }
        [HttpPost("collector/reminder")]
        public async Task<IActionResult> CollectorUcoReminder() 
        {
            return Ok(); 
        }

    }
}
