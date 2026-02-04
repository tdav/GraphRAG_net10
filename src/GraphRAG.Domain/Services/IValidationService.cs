using GraphRAG.Domain.Entities.Medical;

namespace GraphRAG.Domain.Services;

/// <summary>
/// Domain service for validation rules.
/// </summary>
public interface IValidationService
{
    Task<ValidationResult> ValidatePatientAsync(Patient patient, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateQueryAsync(string query, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateFhirBundleAsync(string bundleJson, CancellationToken cancellationToken = default);
}
