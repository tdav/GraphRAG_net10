using GraphRAG.Application.DTOs;

namespace GraphRAG.Application.Interfaces;

/// <summary>
/// Main service interface for GraphRAG operations
/// </summary>
public interface IGraphRagService
{
    /// <summary>
    /// Process a query using the GraphRAG pipeline
    /// </summary>
    Task<QueryResponse> ProcessQueryAsync(QueryRequest request, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate embeddings for text
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract medical entities from text
    /// </summary>
    Task<List<ExtractedEntity>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extracted entity from text
/// </summary>
public record ExtractedEntity
{
    public string Text { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public double Confidence { get; init; }
}
