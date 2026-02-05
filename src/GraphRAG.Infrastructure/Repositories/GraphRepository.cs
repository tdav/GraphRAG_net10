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

        // Set the search path to include the AGE graph and our schema
        await using var setPathCommand = new NpgsqlCommand("SET search_path = ag_catalog, graphrag, public;", connection);
        await setPathCommand.ExecuteNonQueryAsync(cancellationToken);

        // Load the AGE extension if not already loaded (safe to call multiple times)
        await using var loadAgeCommand = new NpgsqlCommand("SELECT load_age_extension();", connection);
        try { await loadAgeCommand.ExecuteNonQueryAsync(cancellationToken); } catch { /* Ignore if already loaded or if function doesn't exist yet */ }

        // Execute the Cypher query via ag_catalog.cypher
        var ageCypherQuery = $"SELECT * FROM ag_catalog.cypher('medical_graph', $cypher$) as (result agtype);";
        
        await using var command = new NpgsqlCommand(ageCypherQuery.Replace("$cypher$", cypherQuery), connection);
        
        // Handle parameters if provided (AGE requires complex parameterization, 
        // for now we'll stick to a slightly more robust string replacement for the query itself, 
        // but in a real system we'd use ag_catalog.age_prepare and execution)
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            var jsonResult = reader.GetString(0);
            try 
            {
                var item = JsonSerializer.Deserialize<T>(jsonResult, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                if (item != null)
                {
                    results.Add(item);
                }
            }
            catch
            {
                // If it's not a direct deserialization (e.g. primitive type), try casting
                var result = reader.GetValue(0);
                if (result is T typedResult)
                {
                    results.Add(typedResult);
                }
            }
        }

        return results;
    }

    public async Task<GraphNode> AddNodeAsync(GraphNode node, CancellationToken cancellationToken = default)
    {
        // 1. Save to relational table for metadata and EF tracking
        _context.GraphNodes.Add(node);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Sync to Apache AGE
        var propsJson = JsonSerializer.Serialize(node.Properties ?? new Dictionary<string, object>());
        var cypherQuery = $"CREATE (n:{node.Label} {{id: '{node.Id}', tenant_id: '{node.TenantId}', properties: '{propsJson}'}})";
        
        await ExecuteCypherQueryAsync<object>(cypherQuery, null, cancellationToken);

        return node;
    }

    public async Task<GraphEdge> AddEdgeAsync(GraphEdge edge, CancellationToken cancellationToken = default)
    {
        // 1. Save to relational table
        _context.GraphEdges.Add(edge);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Sync to Apache AGE
        var propsJson = JsonSerializer.Serialize(edge.Properties ?? new Dictionary<string, object>());
        var cypherQuery = $@"
            MATCH (a {{id: '{edge.SourceNodeId}'}}), (b {{id: '{edge.TargetNodeId}'}})
            CREATE (a)-[r:{edge.EdgeType} {{id: '{edge.Id}', tenant_id: '{edge.TenantId}', weight: {edge.Weight}, properties: '{propsJson}'}}]->(b)";
        
        await ExecuteCypherQueryAsync<object>(cypherQuery, null, cancellationToken);

        return edge;
    }

    public async Task<(IEnumerable<GraphNode> Nodes, IEnumerable<GraphEdge> Edges)> GetSubgraphAsync(
        Guid nodeId, 
        int maxHops, 
        CancellationToken cancellationToken = default)
    {
        // Use Cypher to get connected nodes and edges
        var cypherQuery = $@"
            MATCH path = (start {{id: '{nodeId}'}})-[*1..{maxHops}]-(connected)
            RETURN nodes(path) as nodes, relationships(path) as edges";

        // Note: Parsing complex paths from agtype is non-trivial. 
        // For Phase II, we'll fetch IDs from graph and then hydrate from SQL for performance and reliability.
        
        var nodeIdsQuery = $@"
            MATCH (start {{id: '{nodeId}'}})-[*0..{maxHops}]-(connected)
            RETURN DISTINCT connected.id as id";
        
        var nodeIds = await ExecuteCypherQueryAsync<string>(nodeIdsQuery, null, cancellationToken);
        var guidIds = nodeIds.Select(id => Guid.Parse(id.Trim('"'))).ToList();

        var nodes = await _context.GraphNodes
            .Where(n => guidIds.Contains(n.Id) && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        var edges = await _context.GraphEdges
            .Where(e => guidIds.Contains(e.SourceNodeId) && guidIds.Contains(e.TargetNodeId) && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        return (nodes, edges);
    }

    public async Task<IEnumerable<GraphEdge>> FindShortestPathAsync(
        Guid sourceNodeId, 
        Guid targetNodeId, 
        CancellationToken cancellationToken = default)
    {
        var cypherQuery = $@"
            MATCH p = shortestPath((a {{id: '{sourceNodeId}'}})-[*]-(b {{id: '{targetNodeId}'}}))
            RETURN [r in relationships(p) | r.id] as edgeIds";

        var result = await ExecuteCypherQueryAsync<List<string>>(cypherQuery, null, cancellationToken);
        var edgeIds = result.SelectMany(x => x).Select(id => Guid.Parse(id.Trim('"'))).ToList();

        return await _context.GraphEdges
            .Where(e => edgeIds.Contains(e.Id) && !e.IsDeleted)
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
