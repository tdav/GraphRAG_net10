using GraphRAG.Application.DTOs;
using GraphRAG.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GraphRAG.Application.Services;

/// <summary>
/// Default GraphRAG service implementation.
/// </summary>
public class GraphRagService : IGraphRagService
{
    private readonly IHybridSearchService _hybridSearchService;
    private readonly ILogger<GraphRagService> _logger;

    public GraphRagService(
        IHybridSearchService hybridSearchService,
        ILogger<GraphRagService> logger)
    {
        _hybridSearchService = hybridSearchService;
        _logger = logger;
    }

    public async Task<QueryResponse> ProcessQueryAsync(
        QueryRequest request,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException("Query cannot be empty.", nameof(request));
        }

        var searchContext = await _hybridSearchService.HybridSearchAsync(
            request.Query,
            tenantId,
            request.PatientId,
            request.MaxRelevantNodes,
            cancellationToken);

        var topContext = searchContext.CombinedContext
            .OrderByDescending(item => item.RelevanceScore)
            .Take(request.MaxRelevantNodes)
            .ToList();

        var response = new QueryResponse
        {
            Answer = $"Hybrid search completed. Retrieved {topContext.Count} context items.",
            ConfidenceScore = topContext.Count > 0 ? topContext.Average(item => item.RelevanceScore) : 0,
            RelevantNodes = searchContext.GraphResult?.Nodes
                .Take(request.MaxRelevantNodes)
                .Select(node => new RelevantNode
                {
                    NodeId = node.Id,
                    Label = node.Label,
                    Properties = node.Properties,
                    RelevanceScore = topContext.Count > 0 ? topContext[0].RelevanceScore : 0
                })
                .ToList() ?? new List<RelevantNode>(),
            Sources = topContext.Select(item => new SourceReference
            {
                EntityId = Guid.Empty,
                EntityType = item.Source,
                Description = item.Content
            }).ToList(),
            ConversationId = request.ConversationId,
            Explanation = request.IncludeExplanation
                ? new ExplanationResult
                {
                    Summary = "Hybrid search context assembled. Full reasoning will be implemented in Phase II.",
                    ReasoningSteps = new List<ReasoningStep>()
                }
                : null
        };

        _logger.LogInformation("Processed query for tenant {TenantId} with {ContextCount} context items.",
            tenantId,
            topContext.Count);

        return response;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty.", nameof(text));
        }

        _logger.LogInformation("Generating placeholder embedding for text length {Length}", text.Length);

        return new[] { 0.1f, 0.2f, 0.3f, 0.4f };
    }

    public async Task<List<ExtractedEntity>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<ExtractedEntity>();
        }

        return new List<ExtractedEntity>
        {
            new()
            {
                Text = text.Split(' ').FirstOrDefault() ?? text,
                Type = "Placeholder",
                Confidence = 0.5
            }
        };
    }
}
