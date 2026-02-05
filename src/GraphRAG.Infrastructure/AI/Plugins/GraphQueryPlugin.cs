using System.ComponentModel;
using Microsoft.SemanticKernel;
using GraphRAG.Domain.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GraphRAG.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin for querying the Apache AGE knowledge graph.
/// </summary>
public class GraphQueryPlugin
{
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<GraphQueryPlugin> _logger;

    public GraphQueryPlugin(IGraphRepository graphRepository, ILogger<GraphQueryPlugin> logger)
    {
        _graphRepository = graphRepository;
        _logger = logger;
    }

    [KernelFunction("execute_cypher")]
    [Description("Executes a Cypher query against the medical knowledge graph. Use this to find relationships between patients, conditions, and medications.")]
    public async Task<string> ExecuteCypher(
        [Description("The Cypher query to execute")] string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing Cypher via plugin: {Query}", query);
            var results = await _graphRepository.ExecuteCypherQueryAsync<object>(query, null, cancellationToken);
            return JsonSerializer.Serialize(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Cypher via plugin");
            return $"Error: {ex.Message}";
        }
    }

    [KernelFunction("get_patient_context")]
    [Description("Retrieves the clinical context (subgraph) for a specific patient, including their conditions and medications.")]
    public async Task<string> GetPatientContext(
        [Description("The internal GUID of the patient")] string patientId,
        [Description("Maximum hops to traverse in the graph")] int maxHops = 2,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(patientId, out var patientGuid))
            {
                return "Error: Invalid patient ID format.";
            }

            _logger.LogInformation("Getting patient context for {PatientId} with {Hops} hops", patientId, maxHops);
            var (nodes, edges) = await _graphRepository.GetSubgraphAsync(patientGuid, maxHops, cancellationToken);
            
            var result = new
            {
                nodes = nodes.Select(n => new { n.Id, n.Label, n.Properties }),
                edges = edges.Select(e => new { e.Id, e.SourceNodeId, e.TargetNodeId, e.EdgeType, e.Weight })
            };

            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient context via plugin");
            return $"Error: {ex.Message}";
        }
    }

    [KernelFunction("find_path")]
    [Description("Finds the shortest path between two clinical entities in the graph.")]
    public async Task<string> FindPath(
        [Description("Source entity internal GUID")] string sourceId,
        [Description("Target entity internal GUID")] string targetId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(sourceId, out var sourceGuid) || !Guid.TryParse(targetId, out var targetGuid))
            {
                return "Error: Invalid ID format.";
            }

            var edges = await _graphRepository.FindShortestPathAsync(sourceGuid, targetGuid, cancellationToken);
            return JsonSerializer.Serialize(edges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding path via plugin");
            return $"Error: {ex.Message}";
        }
    }
}
