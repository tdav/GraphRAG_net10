namespace GraphRAG.Application.UseCases.Interfaces;

/// <summary>
/// Orchestrates the process of importing FHIR data.
/// </summary>
public interface IImportFhirDataUseCase
{
    /// <summary>
    /// Executes the FHIR data import process.
    /// </summary>
    /// <param name="bundleJson">The FHIR bundle JSON string.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the number of successful and failed imports.</returns>
    Task<(int Success, int Failed)> ExecuteAsync(string bundleJson, Guid tenantId, CancellationToken cancellationToken = default);
}
