using GraphRAG.Domain.Entities.Graph;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace GraphRAG.Infrastructure.Repositories;

public class GraphRepository : IGraphRepository
{
    private readonly PostgresDbContext _context;
    private readonly string _connectionString;

    public GraphRepository(PostgresDbContext context, string connectionString)
    {
        _context = context;
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<T>> ExecuteCypherQueryAsync<T>(
        string cypherQuery, 
        object? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Set the search path to include the AGE graph
        await using var setPathCommand = new NpgsqlCommand("SET search_path = ag_catalog, \"$user\", public, graphrag;", connection);
        await setPathCommand.ExecuteNonQueryAsync(cancellationToken);

        // Load the AGE extension
        await using var loadAgeCommand = new NpgsqlCommand("LOAD 'age';", connection);
        await loadAgeCommand.ExecuteNonQueryAsync(cancellationToken);

        // Execute the Cypher query
        var ageCypherQuery = $"SELECT * FROM ag_catalog.cypher('medical_graph', $$ {cypherQuery} $$) as (result agtype);";
        
        await using var command = new NpgsqlCommand(ageCypherQuery, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            var result = reader.GetValue(0);
            // For now, return dynamic results - this would need proper deserialization based on T
            if (result is T typedResult)
            {
                results.Add(typedResult);
            }
        }

        return results;
    }

    public async Task<GraphNode> AddNodeAsync(GraphNode node, CancellationToken cancellationToken = default)
    {
        _context.GraphNodes.Add(node);
        await _context.SaveChangesAsync(cancellationToken);

        // Note: For production, implement proper parameterized Cypher queries
        // Apache AGE integration needs proper escaping/parameterization
        // This is a placeholder implementation - full Apache AGE integration in Phase II
        
        // Also add to Apache AGE graph (TODO: Use proper AGE client with parameters)
        // var cypherQuery = $"CREATE (n:{node.Label} {{id: $id, properties: $props}}) RETURN n";
        // await ExecuteCypherQueryAsync<object>(cypherQuery, new { id = node.Id, props = node.PropertiesJson });

        return node;
    }

    public async Task<GraphEdge> AddEdgeAsync(GraphEdge edge, CancellationToken cancellationToken = default)
    {
        _context.GraphEdges.Add(edge);
        await _context.SaveChangesAsync(cancellationToken);

        // Note: For production, implement proper parameterized Cypher queries
        // Apache AGE integration needs proper escaping/parameterization
        // This is a placeholder implementation - full Apache AGE integration in Phase II
        
        // Also add to Apache AGE graph (TODO: Use proper AGE client with parameters)
        // var cypherQuery = @"MATCH (from {id: $fromId}), (to {id: $toId})
        //                     CREATE (from)-[r:$edgeType {id: $id, weight: $weight, properties: $props}]->(to)
        //                     RETURN r";
        // await ExecuteCypherQueryAsync<object>(cypherQuery, 
        //     new { fromId = edge.SourceNodeId, toId = edge.TargetNodeId, edgeType = edge.EdgeType, ... });

        return edge;
    }

    public async Task<(IEnumerable<GraphNode> Nodes, IEnumerable<GraphEdge> Edges)> GetSubgraphAsync(
        Guid nodeId, 
        int maxHops, 
        CancellationToken cancellationToken = default)
    {
        // Get nodes within maxHops
        var cypherQuery = $@"
            MATCH path = (start {{id: '{nodeId}'}})-[*1..{maxHops}]-(connected)
            RETURN DISTINCT connected";

        // For now, return from PostgreSQL tables
        var nodes = await _context.GraphNodes
            .Where(n => n.Id == nodeId)
            .ToListAsync(cancellationToken);

        var edges = await _context.GraphEdges
            .Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId)
            .ToListAsync(cancellationToken);

        return (nodes, edges);
    }

    public async Task<IEnumerable<GraphEdge>> FindShortestPathAsync(
        Guid sourceNodeId, 
        Guid targetNodeId, 
        CancellationToken cancellationToken = default)
    {
        var cypherQuery = $@"
            MATCH path = shortestPath((source {{id: '{sourceNodeId}'}})-[*]-(target {{id: '{targetNodeId}'}}))
            RETURN relationships(path)";

        // For now, return empty - full implementation requires proper AGE integration
        return await _context.GraphEdges
            .Where(e => (e.SourceNodeId == sourceNodeId && e.TargetNodeId == targetNodeId) || 
                       (e.SourceNodeId == targetNodeId && e.TargetNodeId == sourceNodeId))
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
    {
        var node = await _context.GraphNodes.FindAsync(new object[] { nodeId }, cancellationToken);
        if (node != null)
        {
            node.IsDeleted = true;
            node.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Note: For production, implement proper parameterized Cypher queries
            // Apache AGE integration needs proper escaping/parameterization
            // This is a placeholder implementation - full Apache AGE integration in Phase II
            
            // Also delete from Apache AGE graph (TODO: Use proper AGE client with parameters)
            // var cypherQuery = "MATCH (n {id: $nodeId}) DETACH DELETE n";
            // await ExecuteCypherQueryAsync<object>(cypherQuery, new { nodeId });
        }
    }
}
