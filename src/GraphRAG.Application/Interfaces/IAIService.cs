namespace GraphRAG.Application.Interfaces;

/// <summary>
/// Interface for AI services (Embeddings, Chat, NER)
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Generates an embedding for the given text
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a chat completion for the given prompt
    /// </summary>
    Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts medical entities from the given text
    /// </summary>
    Task<List<string>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default);
}
