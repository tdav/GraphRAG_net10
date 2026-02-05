using System.ComponentModel;
using Microsoft.SemanticKernel;
using GraphRAG.Domain.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using GraphRAG.Application.Interfaces;

namespace GraphRAG.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin for searching vector embeddings.
/// </summary>
public class VectorMemoryPlugin
{
    private readonly IVectorRepository _vectorRepository;
    private readonly IAIService _aiService;
    private readonly ILogger<VectorMemoryPlugin> _logger;

    public VectorMemoryPlugin(
        IVectorRepository vectorRepository, 
        IAIService aiService,
        ILogger<VectorMemoryPlugin> logger)
    {
        _vectorRepository = vectorRepository;
        _aiService = aiService;
        _logger = logger;
    }

    [KernelFunction("search_clinical_records")]
    [Description("Searches clinical records and unstructured text for semantically similar information.")]
    public async Task<string> SearchRecords(
        [Description("The semantic query or description to search for")] string query,
        [Description("The tenant ID to isolate data")] string tenantId,
        [Description("Optional entity type filter (Patient, Condition, etc.)")] string? entityType = null,
        [Description("Number of results to return")] int topK = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(tenantId, out var tenantGuid))
            {
                return "Error: Invalid tenant ID.";
            }

            _logger.LogInformation("Vector search via plugin: {Query}", query);
            
            // Generate embedding for the query
            var queryVector = await _aiService.GenerateEmbeddingAsync(query, cancellationToken);
            
            // Search similar
            var results = await _vectorRepository.SearchSimilarAsync(queryVector, topK, tenantGuid, entityType, cancellationToken);
            
            var summary = results.Select(r => new
            {
                r.EntityType,
                r.Text,
                r.MetadataJson
            });

            return JsonSerializer.Serialize(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in vector search plugin");
            return $"Error: {ex.Message}";
        }
    }
}
