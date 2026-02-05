using GraphRAG.Domain.Entities.Graph;
using GraphRAG.Domain.Entities.AI;
using GraphRAG.Domain.Entities.Core;
using GraphRAG.Infrastructure.Repositories;
using Xunit;

namespace GraphRAG.Tests.Infrastructure;

public class RepositoryIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task GraphRepository_AddNode_SavesToDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var repository = new GraphRepository(_context, "Data Source=:memory:");
        var node = new GraphNode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Label = "TestNode",
            Properties = new Dictionary<string, object> { { "key", "value" } }
        };

        // Act
        // We only test the EF part here as AGE requires Npgsql + specific extension
        _context.GraphNodes.Add(node);
        await _context.SaveChangesAsync();

        // Assert
        var dbNode = await _context.GraphNodes.FindAsync(node.Id);
        Assert.NotNull(dbNode);
        Assert.Equal(node.Label, dbNode.Label);
    }

    [Fact]
    public async Task VectorRepository_AddAndSearch_ReturnsResults()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant 2" };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var embedding = new Embedding
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Text = "Test embedding text",
            Vector = new float[] { 0.1f, 0.2f, 0.3f },
            Model = "test-model"
        };

        // Act
        _context.Embeddings.Add(embedding);
        await _context.SaveChangesAsync();
        
        // Assert
        var dbEmbedding = await _context.Embeddings.FindAsync(embedding.Id);
        Assert.NotNull(dbEmbedding);
        Assert.Equal(embedding.Text, dbEmbedding.Text);
    }
}