namespace GraphRAG.Application.Configuration;

/// <summary>
/// Microsoft Semantic Kernel configuration
/// </summary>
public class SemanticKernelSettings
{
    /// <summary>
    /// Azure OpenAI endpoint
    /// </summary>
    public string AzureOpenAIEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string AzureOpenAIApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Chat completion deployment name
    /// </summary>
    public string ChatDeploymentName { get; set; } = "gpt-4";

    /// <summary>
    /// Embedding deployment name
    /// </summary>
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-3-large";

    /// <summary>
    /// API version
    /// </summary>
    public string ApiVersion { get; set; } = "2024-02-01";

    /// <summary>
    /// Max tokens for completion
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// Temperature for completion (0-1)
    /// </summary>
    public double Temperature { get; set; } = 0.7;
}
