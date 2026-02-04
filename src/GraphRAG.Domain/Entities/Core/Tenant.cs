namespace GraphRAG.Domain.Entities.Core;

/// <summary>
/// Tenant entity representing an organization (hospital, clinic)
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Tenant name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Is tenant active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Tenant configuration (JSON)
    /// </summary>
    public string? Configuration { get; set; }

    public Tenant()
    {
        IsActive = true;
    }
}
