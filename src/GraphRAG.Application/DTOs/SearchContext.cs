namespace GraphRAG.Application.DTOs;

/// <summary>
/// Context information from hybrid search (vector + graph)
/// </summary>
public record SearchContext
{
    /// <summary>
    /// Text content retrieved from vector search
    /// </summary>
    public List<VectorSearchResult> VectorResults { get; init; } = new();

    /// <summary>
    /// Graph structure retrieved from graph traversal
    /// </summary>
    public GraphSearchResult? GraphResult { get; init; }

    /// <summary>
    /// Combined and ranked results
    /// </summary>
    public List<ContextItem> CombinedContext { get; init; } = new();
}

/// <summary>
/// Result from vector similarity search
/// </summary>
public record VectorSearchResult
{
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public double SimilarityScore { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Result from graph traversal
/// </summary>
public record GraphSearchResult
{
    public List<GraphNodeInfo> Nodes { get; init; } = new();
    public List<GraphEdgeInfo> Edges { get; init; } = new();
}

public record GraphNodeInfo
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public Dictionary<string, object> Properties { get; init; } = new();
}

public record GraphEdgeInfo
{
    public Guid Id { get; init; }
    public Guid SourceNodeId { get; init; }
    public Guid TargetNodeId { get; init; }
    public string EdgeType { get; init; } = string.Empty;
    public double? Weight { get; init; }
}

/// <summary>
/// Combined context item for LLM input
/// </summary>
public record ContextItem
{
    public string Content { get; init; } = string.Empty;
    public double RelevanceScore { get; init; }
    public string Source { get; init; } = string.Empty;
}
