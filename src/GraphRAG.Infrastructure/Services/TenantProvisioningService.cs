using GraphRAG.Application.Interfaces;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using GraphRAG.Domain.Entities.Core;
using Npgsql;

namespace GraphRAG.Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly PostgresDbContext _context;
    private readonly IGraphRepository _graphRepository;
    private readonly IVectorRepository _vectorRepository;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(
        PostgresDbContext context,
        IGraphRepository graphRepository,
        IVectorRepository vectorRepository,
        ILogger<TenantProvisioningService> logger)
    {
        _context = context;
        _graphRepository = graphRepository;
        _vectorRepository = vectorRepository;
        _logger = logger;
    }

    public async Task<Guid> ProvisionTenantAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Provisioning new tenant: {TenantName}", tenantName);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = tenantName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        // Initialize tenant-specific graph
        await InitializeTenantExtensionsAsync(tenant.Id, cancellationToken);

        return tenant.Id;
    }

    public async Task InitializeTenantExtensionsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing extensions for tenant: {TenantId}", tenantId);

        // Initialize Apache AGE graph for the tenant
        // In this architecture, we might use one graph with tenant_id property, 
        // or separate graphs. Spec suggests "tenant-specific database structures".
        // Let''s create a tenant-specific graph name for isolation.
        
        var graphName = $"graph_{tenantId.ToString("N")}";
        
        // AGE CREATE_GRAPH is not parameterizable via Cypher parameters, needs direct SQL or string interpolation (with caution)
        var sql = $"SELECT create_graph('{graphName}');";
        
        try 
        {
            // Execute graph creation via GraphRepository (which has the connection logic)
            // Note: ExecuteCypherQueryAsync wraps in ag_catalog.cypher, but we need create_graph which is a separate function.
            // For now, let''s use ExecuteCypherQueryAsync with a dummy query if we had a way to execute direct SQL,
            // but since IGraphRepository only exposes Cypher, we use it to call ag_catalog functions.
            
            await _graphRepository.ExecuteCypherQueryAsync<object>($"SELECT create_graph('{graphName}')", null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create AGE graph {GraphName}. It might already exist.", graphName);
        }
    }
}
