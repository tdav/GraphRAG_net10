using GraphRAG.Domain.Entities.Medical;
using GraphRAG.Domain.Services;

namespace GraphRAG.Infrastructure.Services;

/// <summary>
/// Infrastructure placeholder for validation rules.
/// </summary>
public class ValidationService : IValidationService
{
    public Task<ValidationResult> ValidatePatientAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        if (patient is null)
        {
            return Task.FromResult(ValidationResult.Failed("Patient is required."));
        }

        if (string.IsNullOrWhiteSpace(patient.Name))
        {
            return Task.FromResult(ValidationResult.Failed("Patient name is required."));
        }

        return Task.FromResult(ValidationResult.Success());
    }

    public Task<ValidationResult> ValidateQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(ValidationResult.Failed("Query is required."));
        }

        return Task.FromResult(ValidationResult.Success());
    }

    public Task<ValidationResult> ValidateFhirBundleAsync(string bundleJson, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bundleJson))
        {
            return Task.FromResult(ValidationResult.Failed("FHIR bundle content is required."));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}
