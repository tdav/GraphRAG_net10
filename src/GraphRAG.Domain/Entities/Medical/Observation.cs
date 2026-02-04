using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Domain.Entities.Medical;

/// <summary>
/// Observation entity representing a clinical measurement or finding
/// </summary>
public class Observation : BaseEntity
{
    /// <summary>
    /// FHIR Resource ID
    /// </summary>
    public string FhirId { get; set; } = string.Empty;

    /// <summary>
    /// Patient ID this observation is for
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Observation code (LOINC)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Code system (e.g., http://loinc.org)
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Display text for the observation
    /// </summary>
    public string Display { get; set; } = string.Empty;

    /// <summary>
    /// Value (numeric or text)
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Unit of measurement
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Observation status (final, preliminary, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Effective date/time
    /// </summary>
    public DateTime? EffectiveDateTime { get; set; }

    /// <summary>
    /// Additional FHIR data (JSON)
    /// </summary>
    public string? FhirDataJson { get; set; }
}
