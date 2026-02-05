using GraphRAG.Application.Interfaces;
using GraphRAG.Application.UseCases.Interfaces;
using Microsoft.Extensions.Logging;

namespace GraphRAG.Application.UseCases;

public class ImportFhirDataUseCase : IImportFhirDataUseCase
{
    private readonly IFhirEtlService _fhirEtlService;
    private readonly ILogger<ImportFhirDataUseCase> _logger;

    public ImportFhirDataUseCase(IFhirEtlService fhirEtlService, ILogger<ImportFhirDataUseCase> logger)
    {
        _fhirEtlService = fhirEtlService;
        _logger = logger;
    }

    public async Task<(int Success, int Failed)> ExecuteAsync(string bundleJson, Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing ImportFhirDataUseCase for tenant {TenantId}", tenantId);

        if (string.IsNullOrWhiteSpace(bundleJson))
        {
            _logger.LogWarning("Empty FHIR bundle received for tenant {TenantId}", tenantId);
            return (0, 0);
        }

        return await _fhirEtlService.ProcessBundleAsync(bundleJson, tenantId, cancellationToken);
    }
}
