using GraphRAG.Application.DTOs;
using GraphRAG.Application.UseCases.Interfaces;

namespace GraphRAG.Application.UseCases;

public class ExplainReasoningUseCase : IExplainReasoningUseCase
{
    public Task<ExplanationResult> ExecuteAsync(SearchContext searchContext, CancellationToken cancellationToken = default)
    {
        var steps = new List<ReasoningStep>();
        var summary = $"AI analyzed {searchContext.VectorResults.Count} vector results and {searchContext.GraphResult?.Nodes.Count ?? 0} graph nodes.";

        if (searchContext.VectorResults.Any())
        {
            steps.Add(new ReasoningStep
            {
                StepNumber = 1,
                Description = "Identified relevant medical documents using semantic vector search.",
                NodesInvolved = searchContext.VectorResults.Select(r => r.EntityId).ToList()
            });
        }

        if (searchContext.GraphResult?.Nodes.Any() == true)
        {
            steps.Add(new ReasoningStep
            {
                StepNumber = steps.Count + 1,
                Description = "Traversed knowledge graph to find related medical conditions and medications.",
                NodesInvolved = searchContext.GraphResult.Nodes.Select(n => n.Id).ToList()
            });
        }

        return Task.FromResult(new ExplanationResult
        {
            ReasoningSteps = steps,
            Summary = summary,
            AttentionWeights = new List<AttentionInfo>() // Placeholder for Phase III
        });
    }
}
