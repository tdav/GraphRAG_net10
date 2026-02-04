using GraphRAG.Application.DTOs;

namespace GraphRAG.Application.Interfaces;

/// <summary>
/// Service for hybrid search (vector + graph)
/// </summary>
public interface IHybridSearchService
{
    /// <summary>
    /// Perform hybrid search combining vector similarity and graph traversal
    /// </summary>
    Task<SearchContext> HybridSearchAsync(
        string query, 
        Guid tenantId, 
        Guid? patientId = null,
        int maxResults = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform vector similarity search only
    /// </summary>
    Task<List<VectorSearchResult>> VectorSearchAsync(
        float[] queryVector, 
        Guid tenantId,
        int topK = 10,
        string? entityType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform graph traversal from a starting node
    /// </summary>
    Task<GraphSearchResult> GraphTraversalAsync(
        Guid startNodeId,
        int maxHops = 2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Combine and rank results from multiple sources
    /// </summary>
    Task<List<ContextItem>> CombineAndRankResultsAsync(
        List<VectorSearchResult> vectorResults,
        GraphSearchResult? graphResult,
        CancellationToken cancellationToken = default);
}
