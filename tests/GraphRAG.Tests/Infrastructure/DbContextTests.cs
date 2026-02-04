using GraphRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GraphRAG.Tests.Infrastructure;

public class DbContextTests
{
    [Fact]
    public void PostgresDbContext_CanBeCreated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Act
        using var context = new PostgresDbContext(options);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Patients);
        Assert.NotNull(context.Conditions);
        Assert.NotNull(context.GraphNodes);
        Assert.NotNull(context.Embeddings);
    }

    [Fact]
    public void PostgresDbContext_AllDbSetsExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase2")
            .Options;

        // Act & Assert
        using var context = new PostgresDbContext(options);
        
        Assert.NotNull(context.Tenants);
        Assert.NotNull(context.Users);
        Assert.NotNull(context.Conversations);
        Assert.NotNull(context.Patients);
        Assert.NotNull(context.Conditions);
        Assert.NotNull(context.MedicationRequests);
        Assert.NotNull(context.Observations);
        Assert.NotNull(context.GraphNodes);
        Assert.NotNull(context.GraphEdges);
        Assert.NotNull(context.Concepts);
        Assert.NotNull(context.Embeddings);
    }
}
