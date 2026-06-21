using Microsoft.AspNetCore.Mvc;

namespace Api.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                Version = "v2",
                TimeStamp = DateTime.UtcNow
            });
        }
    }
}
