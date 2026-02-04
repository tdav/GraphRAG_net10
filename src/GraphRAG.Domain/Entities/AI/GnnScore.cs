namespace GraphRAG.Domain.Entities.AI;

/// <summary>
/// GnnScore entity representing GNN model score for a graph node
/// </summary>
public class GnnScore
{
    /// <summary>
    /// Node ID
    /// </summary>
    public Guid NodeId { get; set; }

    /// <summary>
    /// Score value (0.0 to 1.0)
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Model version used
    /// </summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when score was computed
    /// </summary>
    public DateTime ComputedAt { get; set; }

    public GnnScore()
    {
        ComputedAt = DateTime.UtcNow;
    }
}
