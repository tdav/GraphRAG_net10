using GraphRAG.Domain.Entities.Medical;

namespace GraphRAG.Domain.Interfaces;

/// <summary>
/// Repository interface for FHIR resource operations
/// </summary>
public interface IFhirRepository
{
    /// <summary>
    /// Import FHIR Patient resource
    /// </summary>
    Task<Patient> ImportPatientAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import FHIR Condition resource
    /// </summary>
    Task<Condition> ImportConditionAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import FHIR MedicationRequest resource
    /// </summary>
    Task<MedicationRequest> ImportMedicationRequestAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import FHIR Observation resource
    /// </summary>
    Task<Observation> ImportObservationAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import FHIR Bundle (multiple resources)
    /// </summary>
    Task<(int Success, int Failed)> ImportBundleAsync(string bundleJson, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patient by FHIR ID
    /// </summary>
    Task<Patient?> GetPatientByFhirIdAsync(string fhirId, Guid tenantId, CancellationToken cancellationToken = default);
}
