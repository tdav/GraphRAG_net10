using GraphRAG.Domain.Entities.Graph;

namespace GraphRAG.Domain.Interfaces;

/// <summary>
/// Repository interface for graph operations (Apache AGE)
/// </summary>
public interface IGraphRepository
{
    /// <summary>
    /// Execute a Cypher query and return results
    /// </summary>
    Task<IEnumerable<T>> ExecuteCypherQueryAsync<T>(string cypherQuery, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a node to the graph
    /// </summary>
    Task<GraphNode> AddNodeAsync(GraphNode node, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add an edge to the graph
    /// </summary>
    Task<GraphEdge> AddEdgeAsync(GraphEdge edge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subgraph around a node (up to N hops)
    /// </summary>
    Task<(IEnumerable<GraphNode> Nodes, IEnumerable<GraphEdge> Edges)> GetSubgraphAsync(
        Guid nodeId, 
        int maxHops, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find shortest path between two nodes
    /// </summary>
    Task<IEnumerable<GraphEdge>> FindShortestPathAsync(
        Guid sourceNodeId, 
        Guid targetNodeId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a node and its edges
    /// </summary>
    Task DeleteNodeAsync(Guid nodeId, CancellationToken cancellationToken = default);
}
