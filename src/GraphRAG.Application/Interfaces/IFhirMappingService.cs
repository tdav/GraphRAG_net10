using Hl7.Fhir.Model;
using GraphRAG.Domain.Entities.Graph;

namespace GraphRAG.Application.Interfaces;

/// <summary>
/// Service for mapping FHIR resources to graph nodes and edges
/// </summary>
public interface IFhirMappingService
{
    /// <summary>
    /// Maps a FHIR Patient to a graph node
    /// </summary>
    GraphNode MapPatient(Patient fhirPatient, Guid tenantId);

    /// <summary>
    /// Maps a FHIR Condition to a graph node and an edge to the patient
    /// </summary>
    (GraphNode Node, GraphEdge Edge) MapCondition(Condition fhirCondition, Guid patientNodeId, Guid tenantId);

    /// <summary>
    /// Maps a FHIR Observation to a graph node and an edge to the patient
    /// </summary>
    (GraphNode Node, GraphEdge Edge) MapObservation(Observation fhirObservation, Guid patientNodeId, Guid tenantId);

    /// <summary>
    /// Maps a FHIR MedicationRequest to a graph node and an edge to the patient
    /// </summary>
    (GraphNode Node, GraphEdge Edge) MapMedicationRequest(MedicationRequest fhirMedication, Guid patientNodeId, Guid tenantId);

    /// <summary>
    /// Extracts edges between conditions and medications or other resources
    /// </summary>
    List<GraphEdge> MapRelationships(Resource resource, Dictionary<string, Guid> fhirIdToInternalId, Guid tenantId);
}
