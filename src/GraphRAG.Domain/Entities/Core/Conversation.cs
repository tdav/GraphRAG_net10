namespace GraphRAG.Domain.Entities.Core;

/// <summary>
/// Conversation entity representing a chat session with the system
/// </summary>
public class Conversation : BaseEntity
{
    /// <summary>
    /// User ID who initiated the conversation
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Conversation title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Messages in this conversation (JSON array)
    /// </summary>
    public string MessagesJson { get; set; } = "[]";

    /// <summary>
    /// Last activity timestamp
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    public Conversation()
    {
        LastActivityAt = DateTime.UtcNow;
    }
}
