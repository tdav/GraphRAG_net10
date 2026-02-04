namespace GraphRAG.Application.DTOs;

/// <summary>
/// Request for querying the GraphRAG system
/// </summary>
public record QueryRequest
{
    /// <summary>
    /// The natural language query from the user
    /// </summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Optional patient ID to scope the query
    /// </summary>
    public Guid? PatientId { get; init; }

    /// <summary>
    /// Optional conversation ID to maintain context
    /// </summary>
    public Guid? ConversationId { get; init; }

    /// <summary>
    /// Additional context or metadata for the query
    /// </summary>
    public Dictionary<string, object>? Context { get; init; }

    /// <summary>
    /// Whether to include explanation of the reasoning
    /// </summary>
    public bool IncludeExplanation { get; init; } = true;

    /// <summary>
    /// Maximum number of relevant nodes to retrieve
    /// </summary>
    public int MaxRelevantNodes { get; init; } = 20;
}
