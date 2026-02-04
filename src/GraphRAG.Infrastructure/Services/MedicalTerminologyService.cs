using GraphRAG.Domain.Services;
using GraphRAG.Domain.ValueObjects;

namespace GraphRAG.Infrastructure.Services;

/// <summary>
/// Infrastructure placeholder for medical terminology normalization.
/// </summary>
public class MedicalTerminologyService : IMedicalTerminologyService
{
    public const string PlaceholderSnomedCode = "999999999";

    public Task<ConceptCode?> NormalizeToSnomedCtAsync(string conceptName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conceptName))
        {
            return Task.FromResult<ConceptCode?>(null);
        }

        // Placeholder non-valid SNOMED CT code for testing; replace with real terminology service integration.
        var code = new ConceptCode("http://snomed.info/sct", PlaceholderSnomedCode, conceptName.Trim());
        return Task.FromResult<ConceptCode?>(code);
    }

    public Task<string> ExpandAcronymAsync(string acronym, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(string.IsNullOrWhiteSpace(acronym) ? string.Empty : acronym.Trim());
    }

    public Task<IReadOnlyList<string>> GetSynonymsAsync(ConceptCode code, CancellationToken cancellationToken = default)
    {
        var synonyms = code.Display is null
            ? Array.Empty<string>()
            : new[] { code.Display };

        return Task.FromResult<IReadOnlyList<string>>(synonyms);
    }
}
