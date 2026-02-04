namespace GraphRAG.Application.Configuration;

/// <summary>
/// GraphRAG specific settings
/// </summary>
public class GraphRagSettings
{
    /// <summary>
    /// Maximum hops for graph traversal
    /// </summary>
    public int MaxGraphHops { get; set; } = 2;

    /// <summary>
    /// Top K results for vector search
    /// </summary>
    public int VectorSearchTopK { get; set; } = 10;

    /// <summary>
    /// Minimum similarity score for vector results (0-1)
    /// </summary>
    public double MinSimilarityScore { get; set; } = 0.7;

    /// <summary>
    /// GNN model path (ONNX)
    /// </summary>
    public string GnnModelPath { get; set; } = string.Empty;

    /// <summary>
    /// Enable GNN scoring
    /// </summary>
    public bool EnableGnnScoring { get; set; } = false;

    /// <summary>
    /// Minimum GNN score threshold
    /// </summary>
    public double MinGnnScore { get; set; } = 0.5;
}
