using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Domain.Entities.Medical;

/// <summary>
/// Patient entity representing a healthcare patient
/// </summary>
public class Patient : BaseEntity
{
    /// <summary>
    /// FHIR Resource ID
    /// </summary>
    public string FhirId { get; set; } = string.Empty;

    /// <summary>
    /// Patient full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Gender (male, female, other, unknown)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Additional FHIR data (JSON)
    /// </summary>
    public string? FhirDataJson { get; set; }
}
