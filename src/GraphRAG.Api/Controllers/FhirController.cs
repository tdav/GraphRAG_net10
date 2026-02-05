using Microsoft.AspNetCore.Mvc;
using GraphRAG.Application.Interfaces;

namespace GraphRAG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FhirController : ControllerBase
{
    private readonly IFhirEtlService _fhirEtlService;
    private readonly ILogger<FhirController> _logger;

    public FhirController(IFhirEtlService fhirEtlService, ILogger<FhirController> logger)
    {
        _fhirEtlService = fhirEtlService;
        _logger = logger;
    }

    /// <summary>
    /// Import a FHIR Bundle (JSON)
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportBundle([FromBody] object bundleJson, [FromQuery] Guid tenantId)
    {
        try
        {
            if (tenantId == Guid.Empty)
            {
                return BadRequest("Invalid tenantId");
            }

            var jsonString = bundleJson.ToString();
            if (string.IsNullOrEmpty(jsonString))
            {
                return BadRequest("Empty bundle");
            }

            _logger.LogInformation("Importing FHIR bundle for tenant {TenantId}", tenantId);
            var (success, failed) = await _fhirEtlService.ProcessBundleAsync(jsonString, tenantId);

            return Ok(new
            {
                message = "FHIR Import completed",
                successCount = success,
                failedCount = failed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing FHIR bundle");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
