using Hl7.Fhir.Model;
using GraphRAG.Application.Interfaces;
using GraphRAG.Domain.Entities.Graph;
using System.Text.Json;

namespace GraphRAG.Infrastructure.Services;

public class FhirMappingService : IFhirMappingService
{
    public GraphNode MapPatient(Patient fhirPatient, Guid tenantId)
    {
        var properties = new Dictionary<string, object>
        {
            ["fhirId"] = fhirPatient.Id ?? string.Empty,
            ["gender"] = fhirPatient.Gender?.ToString() ?? "unknown",
            ["birthDate"] = fhirPatient.BirthDate ?? string.Empty,
            ["active"] = fhirPatient.Active ?? true
        };

        return new GraphNode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Label = "Patient",
            Properties = properties,
            PropertiesJson = JsonSerializer.Serialize(properties),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public (GraphNode Node, GraphEdge Edge) MapCondition(Condition fhirCondition, Guid patientNodeId, Guid tenantId)
    {
        var coding = fhirCondition.Code?.Coding?.FirstOrDefault();
        var properties = new Dictionary<string, object>
        {
            ["fhirId"] = fhirCondition.Id ?? string.Empty,
            ["code"] = coding?.Code ?? string.Empty,
            ["system"] = coding?.System ?? string.Empty,
            ["display"] = coding?.Display ?? fhirCondition.Code?.Text ?? string.Empty,
            ["clinicalStatus"] = fhirCondition.ClinicalStatus?.Coding?.FirstOrDefault()?.Code ?? "unknown"
        };

        var node = new GraphNode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Label = "Condition",
            Properties = properties,
            PropertiesJson = JsonSerializer.Serialize(properties),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var edge = new GraphEdge
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SourceNodeId = patientNodeId,
            TargetNodeId = node.Id,
            EdgeType = "HAS_CONDITION",
            Weight = 1.0,
            Properties = new Dictionary<string, object>(),
            PropertiesJson = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return (node, edge);
    }

    public (GraphNode Node, GraphEdge Edge) MapObservation(Observation fhirObservation, Guid patientNodeId, Guid tenantId)
    {
        var coding = fhirObservation.Code?.Coding?.FirstOrDefault();
        var properties = new Dictionary<string, object>
        {
            ["fhirId"] = fhirObservation.Id ?? string.Empty,
            ["code"] = coding?.Code ?? string.Empty,
            ["system"] = coding?.System ?? string.Empty,
            ["display"] = coding?.Display ?? fhirObservation.Code?.Text ?? string.Empty,
            ["status"] = fhirObservation.Status?.ToString() ?? "unknown"
        };

        if (fhirObservation.Value is Quantity quantity)
        {
            properties["value"] = quantity.Value ?? 0;
            properties["unit"] = quantity.Unit ?? string.Empty;
        }
        else if (fhirObservation.Value is FhirString fhirString)
        {
            properties["value"] = fhirString.Value;
        }

        var node = new GraphNode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Label = "Observation",
            Properties = properties,
            PropertiesJson = JsonSerializer.Serialize(properties),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var edge = new GraphEdge
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SourceNodeId = patientNodeId,
            TargetNodeId = node.Id,
            EdgeType = "HAS_OBSERVATION",
            Weight = 1.0,
            Properties = new Dictionary<string, object>(),
            PropertiesJson = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return (node, edge);
    }

    public (GraphNode Node, GraphEdge Edge) MapMedicationRequest(MedicationRequest fhirMedication, Guid patientNodeId, Guid tenantId)
    {
        var coding = (fhirMedication.Medication as CodeableConcept)?.Coding?.FirstOrDefault();
        var properties = new Dictionary<string, object>
        {
            ["fhirId"] = fhirMedication.Id ?? string.Empty,
            ["code"] = coding?.Code ?? string.Empty,
            ["system"] = coding?.System ?? string.Empty,
            ["display"] = coding?.Display ?? (fhirMedication.Medication as CodeableConcept)?.Text ?? string.Empty,
            ["status"] = fhirMedication.Status?.ToString() ?? "unknown"
        };

        var node = new GraphNode
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Label = "Medication",
            Properties = properties,
            PropertiesJson = JsonSerializer.Serialize(properties),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var edge = new GraphEdge
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SourceNodeId = patientNodeId,
            TargetNodeId = node.Id,
            EdgeType = "PRESCRIBED_MEDICATION",
            Weight = 1.0,
            Properties = new Dictionary<string, object>(),
            PropertiesJson = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return (node, edge);
    }

    public List<GraphEdge> MapRelationships(Resource resource, Dictionary<string, Guid> fhirIdToInternalId, Guid tenantId)
    {
        var edges = new List<GraphEdge>();
        // Here we could implement more complex logic to link conditions to observations, etc.
        // For now, primary links are patient-centric.
        return edges;
    }
}
