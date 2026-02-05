using GraphRAG.Domain.ValueObjects;
using GraphRAG.Domain.Entities.Graph;

namespace GraphRAG.Domain.Services;

/// <summary>
/// Domain service for medical terminology operations.
/// </summary>
public interface IMedicalTerminologyService
{
    Task<ConceptCode?> NormalizeToSnomedCtAsync(string conceptName, CancellationToken cancellationToken = default);
    Task<string> ExpandAcronymAsync(string acronym, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetSynonymsAsync(ConceptCode code, CancellationToken cancellationToken = default);
    
    // Added for Semantic Kernel Plugin
    Task<string> NormalizeTermAsync(string term, CancellationToken cancellationToken = default);
    Task<Concept?> GetConceptByCodeAsync(string code, string system, CancellationToken cancellationToken = default);
}
