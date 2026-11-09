using System.Diagnostics;
using Codium.Template.Application.Contracts.Healths;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Codium.Template.HttpApi.Controllers;

[ApiController]
[Route("health")]
[DisableRateLimiting]
public class HealthController : ControllerBase
{
    private static readonly DateTime StartupTime = DateTime.UtcNow;
    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

    [HttpGet]
    [ProducesResponseType<HealthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult Health()
    {
        var currentTime = DateTime.UtcNow;
        var uptime = currentTime - StartupTime;

        var result = new HealthResponseDto()
        {
            StartupTime = StartupTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            CurrentTime = currentTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            UptimeMs = (long)uptime.TotalMilliseconds,
            UptimePreciseMs = Stopwatch.ElapsedMilliseconds,
            Status = "UP",
        };
       
        return Ok(result);
    }
}