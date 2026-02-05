namespace GraphRAG.Application.Interfaces;

/// <summary>
/// Service for processing FHIR data and distributing it to SQL, Graph, and Vector stores.
/// </summary>
public interface IFhirEtlService
{
    /// <summary>
    /// Processes a FHIR Bundle and imports all supported resources.
    /// </summary>
    Task<(int Success, int Failed)> ProcessBundleAsync(string bundleJson, Guid tenantId, CancellationToken cancellationToken = default);
}
