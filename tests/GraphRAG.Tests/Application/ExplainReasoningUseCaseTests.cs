using GraphRAG.Application.DTOs;
using GraphRAG.Application.UseCases;
using Xunit;

namespace GraphRAG.Tests.Application;

public class ExplainReasoningUseCaseTests
{
    private readonly ExplainReasoningUseCase _useCase;

    public ExplainReasoningUseCaseTests()
    {
        _useCase = new ExplainReasoningUseCase();
    }

    [Fact]
    public async Task ExecuteAsync_WithSearchContext_AssemblesReasoning()
    {
        // Arrange
        var searchContext = new SearchContext
        {
            VectorResults = new List<VectorSearchResult>
            {
                new() { Text = "Semantic match", EntityType = "Condition" }
            },
            GraphResult = new GraphSearchResult
            {
                Nodes = new List<GraphNodeInfo>
                {
                    new() { Id = Guid.NewGuid(), Label = "Patient" },
                    new() { Id = Guid.NewGuid(), Label = "Condition" }
                }
            }
        };

        // Act
        var result = await _useCase.ExecuteAsync(searchContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.ReasoningSteps);
        Assert.Contains("vector", result.Summary);
        Assert.Contains("graph", result.Summary);
    }
}
