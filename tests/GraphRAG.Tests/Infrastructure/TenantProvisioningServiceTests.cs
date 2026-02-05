using GraphRAG.Application.Interfaces;
using GraphRAG.Infrastructure.Services;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.Logging;
using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Tests.Infrastructure;

public class TenantProvisioningServiceTests
{
    private readonly PostgresDbContext _context;
    private readonly IGraphRepository _graphRepository;
    private readonly IVectorRepository _vectorRepository;
    private readonly ILogger<TenantProvisioningService> _logger;
    private readonly TenantProvisioningService _service;

    public TenantProvisioningServiceTests()
    {
        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PostgresDbContext(options);
        
        _graphRepository = Substitute.For<IGraphRepository>();
        _vectorRepository = Substitute.For<IVectorRepository>();
        _logger = Substitute.For<ILogger<TenantProvisioningService>>();
        
        _service = new TenantProvisioningService(
            _context,
            _graphRepository,
            _vectorRepository,
            _logger);
    }

    [Fact]
    public async Task ProvisionTenantAsync_WithValidName_CreatesTenantAndInitializes()
    {
        // Arrange
        var tenantName = "Test Clinic";

        // Act
        var tenantId = await _service.ProvisionTenantAsync(tenantName);

        // Assert
        var tenant = await _context.Tenants.FindAsync(tenantId);
        Assert.NotNull(tenant);
        Assert.Equal(tenantName, tenant.Name);
        
        // Verify graph initialization
        await _graphRepository.Received(1).ExecuteCypherQueryAsync<object>(
            Arg.Is<string>(s => s.Contains("create_graph")),
            null,
            Arg.Any<CancellationToken>());
    }
}

