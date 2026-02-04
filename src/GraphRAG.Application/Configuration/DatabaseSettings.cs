namespace GraphRAG.Application.Configuration;

/// <summary>
/// Database configuration settings
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// PostgreSQL connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Maximum pool size
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Enable sensitive data logging (dev only)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Enable detailed errors (dev only)
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;
}
