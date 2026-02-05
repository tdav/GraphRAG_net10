using GraphRAG.Domain.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphRAG.Domain.Entities.Graph;

/// <summary>
/// GraphEdge entity representing an edge/relationship in the knowledge graph
/// </summary>
public class GraphEdge : BaseEntity
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
    /// Edge type/relationship (HAS_CONDITION, TAKES_MEDICATION, CONTRAINDICATED_WITH, etc.)
    /// </summary>
    public string EdgeType { get; set; } = string.Empty;

    /// <summary>
    /// Edge properties (JSON)
    /// </summary>
    public string PropertiesJson { get; set; } = "{}";

    /// <summary>
    /// Edge properties (NotMapped)
    /// </summary>
    [NotMapped]
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// AGE graph name
    /// </summary>
    public string GraphName { get; set; } = "medical_graph";

    /// <summary>
    /// AGE edge ID (internal)
    /// </summary>
    public long? AgeEdgeId { get; set; }

    /// <summary>
    /// Edge weight/score
    /// </summary>
    public double? Weight { get; set; }
}
