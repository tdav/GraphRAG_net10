using GraphRAG.Domain.Entities.AI;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;

namespace GraphRAG.Infrastructure.Repositories;

public class VectorRepository : IVectorRepository
{
    private readonly PostgresDbContext _context;
    private readonly string _connectionString;

    public VectorRepository(PostgresDbContext context, string connectionString)
    {
        _context = context;
        _connectionString = connectionString;
    }

    public async Task<Embedding> AddEmbeddingAsync(Embedding embedding, CancellationToken cancellationToken = default)
    {
        _context.Embeddings.Add(embedding);
        await _context.SaveChangesAsync(cancellationToken);
        return embedding;
    }

    public async Task<IEnumerable<Embedding>> SearchSimilarAsync(
        float[] queryVector, 
        int topK, 
        Guid tenantId, 
        string? entityType = null, 
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var entityTypeFilter = entityType != null ? "AND entity_type = @entityType" : "";
        var query = $@"
            SELECT id, tenant_id, entity_id, entity_type, text, vector, model, metadata_json, created_at, updated_at
            FROM graphrag.embeddings
            WHERE tenant_id = @tenantId 
              AND is_deleted = false
              {entityTypeFilter}
            ORDER BY vector <=> @vector
            LIMIT @topK";

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@tenantId", tenantId);
        command.Parameters.AddWithValue("@vector", new Vector(queryVector));
        command.Parameters.AddWithValue("@topK", topK);
        
        if (entityType != null)
        {
            command.Parameters.AddWithValue("@entityType", entityType);
        }

        var results = new List<Embedding>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            var embedding = new Embedding
            {
                Id = reader.GetGuid(0),
                TenantId = reader.GetGuid(1),
                EntityId = reader.IsDBNull(2) ? null : reader.GetGuid(2),
                EntityType = reader.IsDBNull(3) ? null : reader.GetString(3),
                Text = reader.GetString(4),
                Vector = reader.IsDBNull(5) ? null : reader.GetFieldValue<float[]>(5),
                Model = reader.GetString(6),
                MetadataJson = reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt = reader.GetDateTime(8),
                UpdatedAt = reader.GetDateTime(9)
            };
            results.Add(embedding);
        }

        return results;
    }

    public async Task<Embedding?> GetByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Embeddings
            .Where(e => e.EntityId == entityId && e.EntityType == entityType && !e.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default)
    {
        var embeddings = await _context.Embeddings
            .Where(e => e.EntityId == entityId && e.EntityType == entityType && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var embedding in embeddings)
        {
            embedding.IsDeleted = true;
            embedding.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Reindex the HNSW index for better performance
        var query = "REINDEX INDEX graphrag.idx_embeddings_vector;";
        await using var command = new NpgsqlCommand(query, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
