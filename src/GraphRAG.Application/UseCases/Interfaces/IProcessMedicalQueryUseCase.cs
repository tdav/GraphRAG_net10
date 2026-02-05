using GraphRAG.Application.DTOs;

namespace GraphRAG.Application.UseCases.Interfaces;

/// <summary>
/// Orchestrates the process of handling a medical query using GraphRAG.
/// </summary>
public interface IProcessMedicalQueryUseCase
{
    /// <summary>
    /// Executes the medical query process.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query response.</returns>
    Task<QueryResponse> ExecuteAsync(QueryRequest request, Guid tenantId, CancellationToken cancellationToken = default);
}
