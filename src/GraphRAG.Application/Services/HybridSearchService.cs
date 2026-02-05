using GraphRAG.Application.Configuration;
using GraphRAG.Application.DTOs;
using GraphRAG.Application.Interfaces;
using GraphRAG.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GraphRAG.Application.Services;

/// <summary>
/// Hybrid search service combining vector and graph retrieval.
/// </summary>
public class HybridSearchService : IHybridSearchService
{
    private readonly IVectorRepository _vectorRepository;
    private readonly IGraphRepository _graphRepository;
    private readonly IAIService _aiService;
    private readonly ILogger<HybridSearchService> _logger;
    private readonly GraphRagSettings _settings;

    public HybridSearchService(
        IVectorRepository vectorRepository,
        IGraphRepository graphRepository,
        IAIService aiService,
        IOptions<GraphRagSettings> settings,
        ILogger<HybridSearchService> logger)
    {
        _vectorRepository = vectorRepository;
        _graphRepository = graphRepository;
        _aiService = aiService;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<SearchContext> HybridSearchAsync(
        string query,
        Guid tenantId,
        Guid? patientId = null,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        if (maxResults < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxResults), "maxResults cannot be negative.");
        }

        var vector = await _aiService.GenerateEmbeddingAsync(query, cancellationToken);
        var vectorResults = maxResults == 0
            ? new List<VectorSearchResult>()
            : await VectorSearchAsync(vector, tenantId, maxResults, null, cancellationToken);

        GraphSearchResult? graphResult = null;
        if (patientId.HasValue)
        {
            graphResult = await GraphTraversalAsync(patientId.Value, _settings.MaxGraphHops, cancellationToken);
        }

        var combined = await CombineAndRankResultsAsync(vectorResults, graphResult, cancellationToken);

        _logger.LogInformation("Hybrid search completed with {VectorCount} vector results and {GraphCount} graph nodes.",
            vectorResults.Count,
            graphResult?.Nodes.Count ?? 0);

        return new SearchContext
        {
            VectorResults = vectorResults,
            GraphResult = graphResult,
            CombinedContext = combined
        };
    }

    public async Task<List<VectorSearchResult>> VectorSearchAsync(
        float[] queryVector,
        Guid tenantId,
        int topK = 10,
        string? entityType = null,
        CancellationToken cancellationToken = default)
    {
        var embeddings = await _vectorRepository.SearchSimilarAsync(queryVector, topK, tenantId, entityType, cancellationToken);
        return embeddings.Select(embedding => new VectorSearchResult
        {
            EntityId = embedding.EntityId ?? Guid.Empty,
            EntityType = embedding.EntityType ?? "Unknown",
            Text = embedding.Text,
            SimilarityScore = 1.0,
            Metadata = new Dictionary<string, object>
            {
                ["model"] = embedding.Model
            }
        }).ToList();
    }

    public async Task<GraphSearchResult> GraphTraversalAsync(
        Guid startNodeId,
        int maxHops = 2,
        CancellationToken cancellationToken = default)
    {
        var (nodes, edges) = await _graphRepository.GetSubgraphAsync(startNodeId, maxHops, cancellationToken);

        return new GraphSearchResult
        {
            Nodes = nodes.Select(node => new GraphNodeInfo
            {
                Id = node.Id,
                Label = node.Label,
                Properties = new Dictionary<string, object>
                {
                    ["graphName"] = node.GraphName
                }
            }).ToList(),
            Edges = edges.Select(edge => new GraphEdgeInfo
            {
                Id = edge.Id,
                SourceNodeId = edge.SourceNodeId,
                TargetNodeId = edge.TargetNodeId,
                EdgeType = edge.EdgeType,
                Weight = edge.Weight
            }).ToList()
        };
    }

    public async Task<List<ContextItem>> CombineAndRankResultsAsync(
        List<VectorSearchResult> vectorResults,
        GraphSearchResult? graphResult,
        CancellationToken cancellationToken = default)
    {
        var combined = new List<ContextItem>();
        combined.AddRange(vectorResults.Select(result => new ContextItem
        {
            Content = result.Text,
            RelevanceScore = result.SimilarityScore,
            Source = $"vector:{result.EntityType}"
        }));

        if (graphResult != null)
        {
            combined.AddRange(graphResult.Nodes.Select(node => new ContextItem
            {
                Content = $"{node.Label} ({node.Id})",
                RelevanceScore = 0.5,
                Source = "graph"
            }));
        }

        return combined.OrderByDescending(item => item.RelevanceScore).ToList();
    }
}
