using GraphRAG.Domain.Events;
using Xunit;

namespace GraphRAG.Tests.Domain;

public class DomainEventTests
{
    [Fact]
    public void PatientImportedEvent_StoresProperties()
    {
        var occurredAt = DateTime.UtcNow;
        var evt = new PatientImportedEvent(Guid.NewGuid(), Guid.NewGuid(), "patient-1", occurredAt);

        Assert.Equal("patient-1", evt.FhirPatientId);
        Assert.Equal(occurredAt, evt.OccurredAt);
    }

    [Fact]
    public void GraphNodeCreatedEvent_StoresProperties()
    {
        var occurredAt = DateTime.UtcNow;
        var evt = new GraphNodeCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Patient", 42, occurredAt);

        Assert.Equal("Patient", evt.NodeLabel);
        Assert.Equal(42, evt.GraphVertexId);
    }

    [Fact]
    public void QueryCompletedEvent_StoresProperties()
    {
        var occurredAt = DateTime.UtcNow;
        var duration = TimeSpan.FromSeconds(2);
        var evt = new QueryCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), "query", "answer", duration, 4, occurredAt);

        Assert.Equal(duration, evt.ProcessingTime);
        Assert.Equal(4, evt.NodesRetrieved);
    }
}
