namespace GraphRAG.Domain.Events;

/// <summary>
/// Domain event emitted after a query is completed.
/// </summary>
public record QueryCompletedEvent(
    Guid ConversationId,
    Guid UserId,
    string Query,
    string Response,
    TimeSpan ProcessingTime,
    int NodesRetrieved,
    DateTime OccurredAt) : IDomainEvent;
