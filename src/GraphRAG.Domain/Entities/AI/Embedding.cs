using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Domain.Entities.AI;

/// <summary>
/// Embedding entity representing a vector embedding for semantic search
/// </summary>
public class Embedding : BaseEntity
{
    /// <summary>
    /// Text that was embedded
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Embedding vector (will be stored as pgvector column)
    /// </summary>
    public float[]? Vector { get; set; }

    /// <summary>
    /// Model used for embedding (e.g., text-embedding-3-large)
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Entity type this embedding belongs to (Patient, Condition, Document, etc.)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Entity ID this embedding belongs to
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Metadata (JSON)
    /// </summary>
    public string? MetadataJson { get; set; }
}
