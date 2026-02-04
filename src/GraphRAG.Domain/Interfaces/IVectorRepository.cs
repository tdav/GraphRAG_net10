using GraphRAG.Domain.Entities.AI;

namespace GraphRAG.Domain.Interfaces;

/// <summary>
/// Repository interface for vector search operations (pgvector)
/// </summary>
public interface IVectorRepository
{
    /// <summary>
    /// Add an embedding
    /// </summary>
    Task<Embedding> AddEmbeddingAsync(Embedding embedding, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for similar embeddings using KNN (K-Nearest Neighbors)
    /// </summary>
    Task<IEnumerable<Embedding>> SearchSimilarAsync(
        float[] queryVector, 
        int topK, 
        Guid tenantId, 
        string? entityType = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get embedding by entity
    /// </summary>
    Task<Embedding?> GetByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete embeddings for an entity
    /// </summary>
    Task DeleteByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuild HNSW index for performance
    /// </summary>
    Task RebuildIndexAsync(CancellationToken cancellationToken = default);
}
