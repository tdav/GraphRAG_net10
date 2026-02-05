using System.ComponentModel;
using Microsoft.SemanticKernel;
using GraphRAG.Domain.Services;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GraphRAG.Infrastructure.AI.Plugins;

/// <summary>
/// Semantic Kernel plugin for medical terminology normalization.
/// </summary>
public class MedicalTerminologyPlugin
{
    private readonly IMedicalTerminologyService _terminologyService;
    private readonly ILogger<MedicalTerminologyPlugin> _logger;

    public MedicalTerminologyPlugin(
        IMedicalTerminologyService terminologyService, 
        ILogger<MedicalTerminologyPlugin> logger)
    {
        _terminologyService = terminologyService;
        _logger = logger;
    }

    [KernelFunction("normalize_medical_term")]
    [Description("Normalizes a medical term or acronym to its standard representation (SNOMED CT, LOINC, RxNorm).")]
    public async Task<string> NormalizeTerm(
        [Description("The term or acronym to normalize")] string term,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Normalizing term: {Term}", term);
            var normalized = await _terminologyService.NormalizeTermAsync(term, cancellationToken);
            return JsonSerializer.Serialize(normalized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error normalizing term via plugin");
            return $"Error: {ex.Message}";
        }
    }

    [KernelFunction("get_concept_details")]
    [Description("Retrieves detailed information about a medical concept by its code and system.")]
    public async Task<string> GetConcept(
        [Description("The standard code (e.g., '44054006')")] string code,
        [Description("The code system (e.g., 'SNOMED-CT')")] string system,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var concept = await _terminologyService.GetConceptByCodeAsync(code, system, cancellationToken);
            return JsonSerializer.Serialize(concept);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting concept via plugin");
            return $"Error: {ex.Message}";
        }
    }
}
