using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Domain.Entities.Medical;

/// <summary>
/// MedicationRequest entity representing a prescription or medication order
/// </summary>
public class MedicationRequest : BaseEntity
{
    /// <summary>
    /// FHIR Resource ID
    /// </summary>
    public string FhirId { get; set; } = string.Empty;

    /// <summary>
    /// Patient ID this medication is for
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Medication code (RxNorm)
    /// </summary>
    public string MedicationCode { get; set; } = string.Empty;

    /// <summary>
    /// Code system (e.g., http://www.nlm.nih.gov/research/umls/rxnorm)
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Medication display name
    /// </summary>
    public string MedicationDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Request status (active, completed, cancelled)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Dosage instructions
    /// </summary>
    public string? DosageInstructions { get; set; }

    /// <summary>
    /// Authored date
    /// </summary>
    public DateTime? AuthoredOn { get; set; }

    /// <summary>
    /// Additional FHIR data (JSON)
    /// </summary>
    public string? FhirDataJson { get; set; }
}
