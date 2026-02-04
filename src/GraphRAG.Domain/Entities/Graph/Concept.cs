using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Domain.Entities.Graph;

/// <summary>
/// Concept entity representing a medical terminology concept (SNOMED CT, LOINC, RxNorm)
/// </summary>
public class Concept : BaseEntity
{
    /// <summary>
    /// Concept code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Code system (SNOMED CT, LOINC, RxNorm, etc.)
    /// </summary>
    public string System { get; set; } = string.Empty;

    /// <summary>
    /// Display text
    /// </summary>
    public string Display { get; set; } = string.Empty;

    /// <summary>
    /// Definition/description
    /// </summary>
    public string? Definition { get; set; }

    /// <summary>
    /// Parent concepts (JSON array of concept IDs)
    /// </summary>
    public string? ParentConceptsJson { get; set; }

    /// <summary>
    /// Embedding vector for semantic search (stored separately)
    /// </summary>
    public Guid? EmbeddingId { get; set; }
}
