using GraphRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace GraphRAG.Tests.Infrastructure;

public class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17") // Or your specific image with AGE/vector
        .Build();

    protected PostgresDbContext _context = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _context = new PostgresDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        
        // Note: Real Apache AGE and pgvector extensions might need a custom image
        // For basic integration tests, we start with standard postgres image
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}
