using GraphRAG.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GraphRAG.Tests.Infrastructure;

public class IntegrationTestBase : IDisposable
{
    protected readonly PostgresDbContext _context;
    private readonly SqliteConnection _connection;

    public IntegrationTestBase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new PostgresDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}