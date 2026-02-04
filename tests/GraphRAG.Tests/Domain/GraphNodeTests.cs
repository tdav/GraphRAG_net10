using GraphRAG.Domain.Entities.Graph;
using Xunit;

namespace GraphRAG.Tests.Domain;

public class GraphNodeTests
{
    [Fact]
    public void GraphNode_WithValidData_CreatesSuccessfully()
    {
        // Arrange & Act
        var node = new GraphNode
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Label = "Patient",
            PropertiesJson = "{\"name\":\"John Doe\"}",
            GraphName = "medical_graph",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, node.Id);
        Assert.Equal("Patient", node.Label);
        Assert.Equal("medical_graph", node.GraphName);
        Assert.Contains("John Doe", node.PropertiesJson);
    }

    [Fact]
    public void GraphNode_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var node = new GraphNode();

        // Assert
        Assert.Equal(string.Empty, node.Label);
        Assert.Equal("{}", node.PropertiesJson);
        Assert.Equal("medical_graph", node.GraphName);
    }

    [Fact]
    public void GraphEdge_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var sourceNodeId = Guid.NewGuid();
        var targetNodeId = Guid.NewGuid();

        // Act
        var edge = new GraphEdge
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
            EdgeType = "HAS_CONDITION",
            Weight = 0.9,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, edge.Id);
        Assert.Equal(sourceNodeId, edge.SourceNodeId);
        Assert.Equal(targetNodeId, edge.TargetNodeId);
        Assert.Equal("HAS_CONDITION", edge.EdgeType);
        Assert.Equal(0.9, edge.Weight);
    }
}
