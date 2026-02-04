namespace GraphRAG.Domain.Events;

/// <summary>
/// Domain event emitted after a graph node is created.
/// </summary>
public record GraphNodeCreatedEvent(
    Guid NodeId,
    Guid TenantId,
    string NodeLabel,
    long? GraphVertexId,
    DateTime OccurredAt) : IDomainEvent;
