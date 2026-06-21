using Api.Models;
using Api.services;
using Microsoft.AspNetCore.Mvc;

namespace Api.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentAnalysisController : ControllerBase
    {
        public readonly IncidentAnalysisService _service;

        public IncidentAnalysisController(IncidentAnalysisService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> Analyze(IncidentAnalysisRequest request)
        {
            var response = await _service.Analyze(request);
            return Ok(response);
        }
    }
}
