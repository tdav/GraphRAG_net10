using GraphRAG.Domain.Entities.Core;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraphRAG.Infrastructure.Repositories;

public class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    public ConversationRepository(PostgresDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(
        Guid userId, 
        int pageSize = 20, 
        int page = 1, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddMessageAsync(
        Guid conversationId, 
        string role, 
        string content, 
        CancellationToken cancellationToken = default)
    {
        var conversation = await GetByIdAsync(conversationId, cancellationToken);
        if (conversation != null)
        {
            // Parse existing messages, add new one, and serialize back
            var messages = new List<object>();
            if (!string.IsNullOrEmpty(conversation.MessagesJson) && conversation.MessagesJson != "[]")
            {
                messages = System.Text.Json.JsonSerializer.Deserialize<List<object>>(conversation.MessagesJson) ?? new List<object>();
            }
            
            messages.Add(new { role, content, timestamp = DateTime.UtcNow });
            conversation.MessagesJson = System.Text.Json.JsonSerializer.Serialize(messages);
            
            conversation.UpdatedAt = DateTime.UtcNow;
            conversation.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Conversation>> GetRecentByTenantAsync(
        Guid tenantId, 
        int limit = 10, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .OrderByDescending(c => c.UpdatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
