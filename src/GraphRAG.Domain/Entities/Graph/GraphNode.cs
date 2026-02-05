using GraphRAG.Domain.Entities.Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraphRAG.Domain.Entities.Graph;

/// <summary>
/// GraphNode entity representing a node in the knowledge graph
/// </summary>
public class GraphNode : BaseEntity
{
    /// <summary>
    /// Node label/type (Patient, Condition, Medication, Concept, etc.)
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Node properties (JSON)
    /// </summary>
    public string PropertiesJson { get; set; } = "{}";

    /// <summary>
    /// Node properties (NotMapped)
    /// </summary>
    [NotMapped]
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// AGE graph name
    /// </summary>
    public string GraphName { get; set; } = "medical_graph";

    /// <summary>
    /// AGE vertex ID (internal)
    /// </summary>
    public long? AgeVertexId { get; set; }
}
