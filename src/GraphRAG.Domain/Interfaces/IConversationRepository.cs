using GraphRAG.Domain.Entities.Core;

namespace GraphRAG.Domain.Interfaces;

/// <summary>
/// Repository interface for conversation operations
/// </summary>
public interface IConversationRepository : IRepository<Conversation>
{
    /// <summary>
    /// Get conversations for a user
    /// </summary>
    Task<IEnumerable<Conversation>> GetByUserIdAsync(
        Guid userId, 
        int pageSize = 20, 
        int page = 1, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add message to conversation
    /// </summary>
    Task AddMessageAsync(
        Guid conversationId, 
        string role, 
        string content, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent conversations by tenant
    /// </summary>
    Task<IEnumerable<Conversation>> GetRecentByTenantAsync(
        Guid tenantId, 
        int limit = 10, 
        CancellationToken cancellationToken = default);
}
