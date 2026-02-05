namespace GraphRAG.Application.Interfaces;

/// <summary>
/// Service for dynamic tenant provisioning and schema initialization.
/// </summary>
public interface ITenantProvisioningService
{
    /// <summary>
    /// Provisions a new tenant, including database records and schema initialization.
    /// </summary>
    /// <param name="tenantName">The name of the new tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created tenant.</returns>
    Task<Guid> ProvisionTenantAsync(string tenantName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes Apache AGE and pgvector extensions for a specific tenant if needed.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitializeTenantExtensionsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
