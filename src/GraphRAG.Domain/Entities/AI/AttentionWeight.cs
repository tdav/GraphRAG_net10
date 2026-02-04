namespace GraphRAG.Domain.Entities.AI;

/// <summary>
/// AttentionWeight entity representing GAT attention weights for explainability
/// </summary>
public class AttentionWeight
{
    /// <summary>
    /// Source node ID
    /// </summary>
    public Guid SourceNodeId { get; set; }

    /// <summary>
    /// Target node ID
    /// </summary>
    public Guid TargetNodeId { get; set; }

    /// <summary>
    /// Attention weight value (0.0 to 1.0)
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// GAT layer this weight is from
    /// </summary>
    public int Layer { get; set; }

    /// <summary>
    /// Attention head index
    /// </summary>
    public int Head { get; set; }

    /// <summary>
    /// Query ID this weight is associated with
    /// </summary>
    public Guid? QueryId { get; set; }
}
