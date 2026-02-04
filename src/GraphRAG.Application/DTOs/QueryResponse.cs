namespace GraphRAG.Application.DTOs;

/// <summary>
/// Response from the GraphRAG system
/// </summary>
public record QueryResponse
{
    /// <summary>
    /// The generated answer to the user's query
    /// </summary>
    public string Answer { get; init; } = string.Empty;

    /// <summary>
    /// Relevant nodes from the knowledge graph
    /// </summary>
    public List<RelevantNode> RelevantNodes { get; init; } = new();

    /// <summary>
    /// Explanation of the reasoning process (XAI)
    /// </summary>
    public ExplanationResult? Explanation { get; init; }

    /// <summary>
    /// Sources used to generate the answer
    /// </summary>
    public List<SourceReference> Sources { get; init; } = new();

    /// <summary>
    /// Conversation ID for maintaining context
    /// </summary>
    public Guid? ConversationId { get; init; }

    /// <summary>
    /// Confidence score for the answer (0-1)
    /// </summary>
    public double ConfidenceScore { get; init; }
}

/// <summary>
/// Represents a relevant node from the knowledge graph
/// </summary>
public record RelevantNode
{
    public Guid NodeId { get; init; }
    public string Label { get; init; } = string.Empty;
    public Dictionary<string, object> Properties { get; init; } = new();
    public double RelevanceScore { get; init; }
}

/// <summary>
/// Explanation of the AI reasoning
/// </summary>
public record ExplanationResult
{
    public List<ReasoningStep> ReasoningSteps { get; init; } = new();
    public List<AttentionInfo> AttentionWeights { get; init; } = new();
    public string Summary { get; init; } = string.Empty;
}

/// <summary>
/// A step in the reasoning process
/// </summary>
public record ReasoningStep
{
    public int StepNumber { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<Guid> NodesInvolved { get; init; } = new();
}

/// <summary>
/// Attention weight information for XAI
/// </summary>
public record AttentionInfo
{
    public Guid SourceNodeId { get; init; }
    public Guid TargetNodeId { get; init; }
    public double Weight { get; init; }
}

/// <summary>
/// Reference to a source document or entity
/// </summary>
public record SourceReference
{
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
