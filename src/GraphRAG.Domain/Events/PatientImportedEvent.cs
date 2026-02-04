namespace GraphRAG.Domain.Events;

/// <summary>
/// Domain event emitted after a patient is imported from FHIR.
/// </summary>
public record PatientImportedEvent(
    Guid PatientId,
    Guid TenantId,
    string FhirPatientId,
    DateTime OccurredAt) : IDomainEvent;
