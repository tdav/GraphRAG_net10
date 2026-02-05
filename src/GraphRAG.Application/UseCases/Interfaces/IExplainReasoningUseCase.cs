using GraphRAG.Application.DTOs;

namespace GraphRAG.Application.UseCases.Interfaces;

/// <summary>
/// Orchestrates the process of explaining the AI reasoning.
/// </summary>
public interface IExplainReasoningUseCase
{
    /// <summary>
    /// Assembles reasoning paths and explanation from search context.
    /// </summary>
    /// <param name="searchContext">The search context containing vector and graph results.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The explanation result.</returns>
    Task<ExplanationResult> ExecuteAsync(SearchContext searchContext, CancellationToken cancellationToken = default);
}
