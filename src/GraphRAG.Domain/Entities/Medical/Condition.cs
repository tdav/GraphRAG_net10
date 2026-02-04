using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Domain.Entities.Medical;

/// <summary>
/// Condition entity representing a patient diagnosis or health condition
/// </summary>
public class Condition : BaseEntity
{
    /// <summary>
    /// FHIR Resource ID
    /// </summary>
    public string FhirId { get; set; } = string.Empty;

    /// <summary>
    /// Patient ID this condition belongs to
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Condition code (SNOMED CT)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Code system (e.g., http://snomed.info/sct)
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Display text for the condition
    /// </summary>
    public string Display { get; set; } = string.Empty;

    /// <summary>
    /// Clinical status (active, resolved, etc.)
    /// </summary>
    public string? ClinicalStatus { get; set; }

    /// <summary>
    /// Onset date
    /// </summary>
    public DateTime? OnsetDate { get; set; }

    /// <summary>
    /// Additional FHIR data (JSON)
    /// </summary>
    public string? FhirDataJson { get; set; }
}
