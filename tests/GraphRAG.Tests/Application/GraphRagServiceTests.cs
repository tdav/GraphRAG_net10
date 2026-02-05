using GraphRAG.Application.DTOs;
using GraphRAG.Application.Interfaces;
using GraphRAG.Application.Services;
using GraphRAG.Domain.Entities.AI;
using GraphRAG.Domain.Entities.Graph;
using GraphRAG.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GraphRAG.Tests.Application;

public class GraphRagServiceTests
{
    [Fact]
    public async Task ProcessQueryAsync_ReturnsResponseWithContext()
    {
        var hybridSearchService = new StubHybridSearchService();
        var aiService = new StubAIService();
        var service = new GraphRagService(hybridSearchService, aiService, NullLogger<GraphRagService>.Instance);

        var response = await service.ProcessQueryAsync(new QueryRequest { Query = "Test query" }, Guid.NewGuid());

        Assert.Contains("Retrieved 2", response.Explanation?.Summary ?? "");
        Assert.Equal(2, response.Sources.Count);
    }

    private sealed class StubHybridSearchService : IHybridSearchService
    {
        public Task<SearchContext> HybridSearchAsync(
            string query,
            Guid tenantId,
            Guid? patientId = null,
            int maxResults = 20,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SearchContext
            {
                CombinedContext = new List<ContextItem>
                {
                    new() { Content = "Item 1", RelevanceScore = 0.9, Source = "vector" },
                    new() { Content = "Item 2", RelevanceScore = 0.8, Source = "graph" }
                }
            });
        }

        public Task<List<VectorSearchResult>> VectorSearchAsync(
            float[] queryVector,
            Guid tenantId,
            int topK = 10,
            string? entityType = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new List<VectorSearchResult>());

        public Task<GraphSearchResult> GraphTraversalAsync(
            Guid startNodeId,
            int maxHops = 2,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new GraphSearchResult());

        public Task<List<ContextItem>> CombineAndRankResultsAsync(
            List<VectorSearchResult> vectorResults,
            GraphSearchResult? graphResult,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new List<ContextItem>());
    }
}

internal sealed class StubAIService : IAIService
{
    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult(new float[1536]);

    public Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
        => Task.FromResult("AI Response");

    public Task<List<string>> ExtractEntitiesAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<string> { "Entity1" });
}

public class HybridSearchServiceTests
{
    [Fact]
    public async Task HybridSearchAsync_ReturnsCombinedContext()
    {
        var vectorRepository = new StubVectorRepository();
        var graphRepository = new StubGraphRepository();
        var aiService = new StubAIService();
        var options = Options.Create(new GraphRAG.Application.Configuration.GraphRagSettings
        {
            MaxGraphHops = 1
        });
        var service = new HybridSearchService(
            vectorRepository,
            graphRepository,
            aiService,
            options,
            NullLogger<HybridSearchService>.Instance);

        var context = await service.HybridSearchAsync("Query", Guid.NewGuid(), Guid.NewGuid(), 2);

        Assert.NotEmpty(context.CombinedContext);
        Assert.Single(context.VectorResults);
        Assert.NotNull(context.GraphResult);
        Assert.Single(context.GraphResult!.Nodes);
    }

    private sealed class StubVectorRepository : IVectorRepository
    {
        public Task<Embedding> AddEmbeddingAsync(Embedding embedding, CancellationToken cancellationToken = default)
            => Task.FromResult(embedding);

        public Task<IEnumerable<Embedding>> SearchSimilarAsync(
            float[] queryVector,
            int topK,
            Guid tenantId,
            string? entityType = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Embedding>>(new List<Embedding>
            {
                new() { Text = "Vector result", Model = "test" }
            });
        }

        public Task<Embedding?> GetByEntityAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
            => Task.FromResult<Embedding?>(null);

        public Task DeleteByEntityAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RebuildIndexAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubGraphRepository : IGraphRepository
    {
        public Task<IEnumerable<T>> ExecuteCypherQueryAsync<T>(
            string cypherQuery,
            object? parameters = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<T>>(Array.Empty<T>());

        public Task<GraphNode> AddNodeAsync(GraphNode node, CancellationToken cancellationToken = default)
            => Task.FromResult(node);

        public Task<GraphEdge> AddEdgeAsync(GraphEdge edge, CancellationToken cancellationToken = default)
            => Task.FromResult(edge);

        public Task<(IEnumerable<GraphNode> Nodes, IEnumerable<GraphEdge> Edges)> GetSubgraphAsync(
            Guid nodeId,
            int maxHops,
            CancellationToken cancellationToken = default)
        {
            var nodes = new List<GraphNode> { new() { Id = nodeId, Label = "Patient" } };
            return Task.FromResult<(IEnumerable<GraphNode>, IEnumerable<GraphEdge>)>((nodes, new List<GraphEdge>()));
        }

        public Task<IEnumerable<GraphEdge>> FindShortestPathAsync(
            Guid sourceNodeId,
            Guid targetNodeId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<GraphEdge>>(Array.Empty<GraphEdge>());

        public Task DeleteNodeAsync(Guid nodeId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
