using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace GraphRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Get detailed health status of the system
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = healthReport.Status.ToString(),
                totalDuration = healthReport.TotalDuration.TotalMilliseconds,
                checks = healthReport.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description,
                    exception = entry.Value.Exception?.Message
                }),
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown"
            };

            var statusCode = healthReport.Status switch
            {
                HealthStatus.Healthy => 200,
                HealthStatus.Degraded => 200,
                _ => 503
            };

            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health status");
            return StatusCode(500, new
            {
                status = "Unhealthy",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Simple readiness check
    /// </summary>
    [HttpGet("ready")]
    public IActionResult GetReadiness()
    {
        return Ok(new { status = "ready" });
    }

    /// <summary>
    /// Simple liveness check
    /// </summary>
    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        return Ok(new { status = "alive" });
    }
}
