using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using GraphRAG.Application.Interfaces;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Domain.Entities.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GraphRAG.Infrastructure.Services;

public class FhirEtlService : IFhirEtlService
{
    private readonly IFhirRepository _fhirRepository;
    private readonly IGraphRepository _graphRepository;
    private readonly IVectorRepository _vectorRepository;
    private readonly IFhirMappingService _mappingService;
    private readonly IAIService _aiService;
    private readonly ILogger<FhirEtlService> _logger;
    private readonly FhirJsonParser _parser;

    public FhirEtlService(
        IFhirRepository fhirRepository,
        IGraphRepository graphRepository,
        IVectorRepository vectorRepository,
        IFhirMappingService mappingService,
        IAIService aiService,
        ILogger<FhirEtlService> logger)
    {
        _fhirRepository = fhirRepository;
        _graphRepository = graphRepository;
        _vectorRepository = vectorRepository;
        _mappingService = mappingService;
        _aiService = aiService;
        _logger = logger;
        _parser = new FhirJsonParser();
    }

    public async Task<(int Success, int Failed)> ProcessBundleAsync(string bundleJson, Guid tenantId, CancellationToken cancellationToken = default)
    {
        int success = 0;
        int failed = 0;

        try
        {
            var bundle = await _parser.ParseAsync<Bundle>(bundleJson);
            var fhirIdToInternalId = new Dictionary<string, Guid>();

            // First pass: Import Patients (they are the anchors)
            foreach (var entry in bundle.Entry.Where(e => e.Resource is Patient))
            {
                try
                {
                    var patient = (Patient)entry.Resource;
                    var dbPatient = await _fhirRepository.ImportPatientAsync(patient.ToJson(), tenantId, cancellationToken);
                    
                    // Map to Graph
                    var graphNode = _mappingService.MapPatient(patient, tenantId);
                    graphNode.Id = dbPatient.Id; // Keep IDs consistent
                    await _graphRepository.AddNodeAsync(graphNode, cancellationToken);

                    // Map to Vector
                    var embedding = new Embedding
                    {
                        TenantId = tenantId,
                        EntityId = dbPatient.Id,
                        EntityType = "Patient",
                        Text = $"Patient: {dbPatient.Name}, Gender: {dbPatient.Gender}, BirthDate: {dbPatient.BirthDate}",
                        Vector = await _aiService.GenerateEmbeddingAsync(dbPatient.Name, cancellationToken),
                        Model = "text-embedding-3-large"
                    };
                    await _vectorRepository.AddEmbeddingAsync(embedding, cancellationToken);

                    fhirIdToInternalId[patient.Id] = dbPatient.Id;
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import Patient");
                    failed++;
                }
            }

            // Second pass: Import other resources (linked to patients)
            foreach (var entry in bundle.Entry.Where(e => !(e.Resource is Patient)))
            {
                try
                {
                    var resource = entry.Resource;
                    Guid? internalPatientId = null;

                    // Resolve patient reference to get internal ID
                    if (resource is Condition cond && cond.Subject?.Reference != null)
                    {
                        var patientFhirId = cond.Subject.Reference.Replace("Patient/", "");
                        if (fhirIdToInternalId.TryGetValue(patientFhirId, out var id)) internalPatientId = id;
                    }
                    else if (resource is Observation obs && obs.Subject?.Reference != null)
                    {
                        var patientFhirId = obs.Subject.Reference.Replace("Patient/", "");
                        if (fhirIdToInternalId.TryGetValue(patientFhirId, out var id)) internalPatientId = id;
                    }
                    else if (resource is MedicationRequest med && med.Subject?.Reference != null)
                    {
                        var patientFhirId = med.Subject.Reference.Replace("Patient/", "");
                        if (fhirIdToInternalId.TryGetValue(patientFhirId, out var id)) internalPatientId = id;
                    }

                    if (internalPatientId == null)
                    {
                        _logger.LogWarning("Skipping resource {Type}/{Id} because patient reference could not be resolved.", resource.TypeName, resource.Id);
                        failed++;
                        continue;
                    }

                    if (resource is Condition condition)
                    {
                        var dbCondition = await _fhirRepository.ImportConditionAsync(condition.ToJson(), tenantId, cancellationToken);
                        var (node, edge) = _mappingService.MapCondition(condition, internalPatientId.Value, tenantId);
                        node.Id = dbCondition.Id;
                        await _graphRepository.AddNodeAsync(node, cancellationToken);
                        await _graphRepository.AddEdgeAsync(edge, cancellationToken);

                        var embedding = new Embedding
                        {
                            TenantId = tenantId,
                            EntityId = dbCondition.Id,
                            EntityType = "Condition",
                            Text = $"Condition: {dbCondition.Display}, Status: {dbCondition.ClinicalStatus}",
                            Vector = await _aiService.GenerateEmbeddingAsync(dbCondition.Display, cancellationToken),
                            Model = "text-embedding-3-large"
                        };
                        await _vectorRepository.AddEmbeddingAsync(embedding, cancellationToken);
                    }
                    else if (resource is Observation observation)
                    {
                        var dbObservation = await _fhirRepository.ImportObservationAsync(observation.ToJson(), tenantId, cancellationToken);
                        var (node, edge) = _mappingService.MapObservation(observation, internalPatientId.Value, tenantId);
                        node.Id = dbObservation.Id;
                        await _graphRepository.AddNodeAsync(node, cancellationToken);
                        await _graphRepository.AddEdgeAsync(edge, cancellationToken);
                    }
                    else if (resource is MedicationRequest medReq)
                    {
                        var dbMedReq = await _fhirRepository.ImportMedicationRequestAsync(medReq.ToJson(), tenantId, cancellationToken);
                        var (node, edge) = _mappingService.MapMedicationRequest(medReq, internalPatientId.Value, tenantId);
                        node.Id = dbMedReq.Id;
                        await _graphRepository.AddNodeAsync(node, cancellationToken);
                        await _graphRepository.AddEdgeAsync(edge, cancellationToken);
                    }

                    success++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import resource {Type}", entry.Resource.TypeName);
                    failed++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error processing FHIR bundle");
            throw;
        }

        return (success, failed);
    }
}
