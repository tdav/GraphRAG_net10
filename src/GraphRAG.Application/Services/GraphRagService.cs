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
    private readonly IAIService _aiService;
    private readonly ILogger<GraphRagService> _logger;

    public GraphRagService(
        IHybridSearchService hybridSearchService,
        IAIService aiService,
        ILogger<GraphRagService> logger)
    {
        _hybridSearchService = hybridSearchService;
        _aiService = aiService;
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

        // 1. Extract entities (NER)
        var entities = await _aiService.ExtractEntitiesAsync(request.Query, cancellationToken);
        _logger.LogInformation("Extracted entities: {Entities}", string.Join(", ", entities));

        // 2. Perform Hybrid Search
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

        // 3. (Optional) GNN Re-ranking would happen here in Phase III

        // 4. Generate Answer using Context
        var contextText = string.Join("\n", topContext.Select(c => c.Content));
        var prompt = $@"
            You are a medical AI assistant. Answer the patient query based on the provided context.
            If the context does not contain enough information, state that clearly.
            
            Context:
            {contextText}
            
            Query: {request.Query}
            
            Answer:";

        var answer = await _aiService.GetChatCompletionAsync(prompt, cancellationToken);

        var response = new QueryResponse
        {
            Answer = answer,
            ConfidenceScore = topContext.Count > 0 ? topContext.Average(item => item.RelevanceScore) : 0,
            RelevantNodes = searchContext.GraphResult?.Nodes
                .Take(request.MaxRelevantNodes)
                .Select(node => new RelevantNode
                {
                    NodeId = node.Id,
                    Label = node.Label,
                    Properties = node.Properties,
                    RelevanceScore = 1.0 // Simple score for now
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
                    Summary = $"Retrieved {topContext.Count} relevant pieces of clinical information.",
                    ReasoningSteps = new List<ReasoningStep>
                    {
                        new ReasoningStep { Description = "Extracted entities from query." },
                        new ReasoningStep { Description = "Searched vector database for semantic similarity." },
                        new ReasoningStep { Description = "Traversed knowledge graph for relationships." },
                        new ReasoningStep { Description = "Consolidated findings into a clinical answer." }
                    }
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
        return await _aiService.GenerateEmbeddingAsync(text, cancellationToken);
    }

    public async Task<List<ExtractedEntity>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default)
    {
        var entities = await _aiService.ExtractEntitiesAsync(text, cancellationToken);
        return entities.Select(e => new ExtractedEntity
        {
            Text = e,
            Type = "MedicalEntity",
            Confidence = 0.9
        }).ToList();
    }
}
