using GraphRAG.Domain.ValueObjects;

namespace GraphRAG.Domain.Services;

/// <summary>
/// Domain service for medical terminology operations.
/// </summary>
public interface IMedicalTerminologyService
{
    Task<ConceptCode?> NormalizeToSnomedCtAsync(string conceptName, CancellationToken cancellationToken = default);
    Task<string> ExpandAcronymAsync(string acronym, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetSynonymsAsync(ConceptCode code, CancellationToken cancellationToken = default);
}
