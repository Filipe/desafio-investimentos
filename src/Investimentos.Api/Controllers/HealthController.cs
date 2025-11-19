using Microsoft.AspNetCore.Mvc;

namespace Investimentos.Api.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Endpoint de health check
    /// </summary>
    /// <returns>Status 200 OK</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok();
    }
}
