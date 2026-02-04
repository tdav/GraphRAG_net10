using GraphRAG.Domain.Entities.Medical;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GraphRAG.Infrastructure.Repositories;

public class FhirRepository : IFhirRepository
{
    private readonly PostgresDbContext _context;

    public FhirRepository(PostgresDbContext context)
    {
        _context = context;
    }

    public async Task<Patient> ImportPatientAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var fhirData = JsonDocument.Parse(fhirJson);
        var root = fhirData.RootElement;

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FhirId = root.GetProperty("id").GetString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FhirDataJson = fhirJson,
            Name = string.Empty
        };

        // Extract name
        if (root.TryGetProperty("name", out var nameArray) && nameArray.GetArrayLength() > 0)
        {
            var name = nameArray[0];
            var givenNames = new List<string>();
            if (name.TryGetProperty("given", out var givenNamesArray))
            {
                foreach (var given in givenNamesArray.EnumerateArray())
                {
                    var givenName = given.GetString();
                    if (givenName != null) givenNames.Add(givenName);
                }
            }
            
            var familyName = "";
            if (name.TryGetProperty("family", out var family))
            {
                familyName = family.GetString() ?? "";
            }
            
            patient.Name = string.Join(" ", givenNames) + " " + familyName;
        }

        // Extract gender
        if (root.TryGetProperty("gender", out var gender))
        {
            patient.Gender = gender.GetString();
        }

        // Extract birth date
        if (root.TryGetProperty("birthDate", out var birthDate))
        {
            if (DateTime.TryParse(birthDate.GetString(), out var date))
            {
                patient.BirthDate = date;
            }
        }

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return patient;
    }

    public async Task<Condition> ImportConditionAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var fhirData = JsonDocument.Parse(fhirJson);
        var root = fhirData.RootElement;

        var condition = new Condition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FhirId = root.GetProperty("id").GetString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FhirDataJson = fhirJson,
            Code = string.Empty,
            CodeSystem = string.Empty,
            Display = string.Empty
        };

        // Extract patient reference
        if (root.TryGetProperty("subject", out var subject) && 
            subject.TryGetProperty("reference", out var reference))
        {
            var refString = reference.GetString();
            if (refString != null && refString.StartsWith("Patient/"))
            {
                var fhirPatientId = refString.Replace("Patient/", "");
                var patient = await GetPatientByFhirIdAsync(fhirPatientId, tenantId, cancellationToken);
                if (patient != null)
                {
                    condition.PatientId = patient.Id;
                }
            }
        }

        // Extract code (SNOMED CT or other)
        if (root.TryGetProperty("code", out var code) && 
            code.TryGetProperty("coding", out var codingArray) && 
            codingArray.GetArrayLength() > 0)
        {
            var coding = codingArray[0];
            if (coding.TryGetProperty("system", out var system))
            {
                condition.CodeSystem = system.GetString() ?? "";
            }
            if (coding.TryGetProperty("code", out var codeValue))
            {
                condition.Code = codeValue.GetString() ?? "";
            }
            if (coding.TryGetProperty("display", out var display))
            {
                condition.Display = display.GetString() ?? "";
            }
        }

        // Extract clinical status
        if (root.TryGetProperty("clinicalStatus", out var clinicalStatus) &&
            clinicalStatus.TryGetProperty("coding", out var statusArray) &&
            statusArray.GetArrayLength() > 0)
        {
            var statusCoding = statusArray[0];
            if (statusCoding.TryGetProperty("code", out var statusCode))
            {
                condition.ClinicalStatus = statusCode.GetString();
            }
        }

        // Extract onset date
        if (root.TryGetProperty("onsetDateTime", out var onsetDateTime))
        {
            if (DateTime.TryParse(onsetDateTime.GetString(), out var date))
            {
                condition.OnsetDate = date;
            }
        }

        _context.Conditions.Add(condition);
        await _context.SaveChangesAsync(cancellationToken);

        return condition;
    }

    public async Task<MedicationRequest> ImportMedicationRequestAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var fhirData = JsonDocument.Parse(fhirJson);
        var root = fhirData.RootElement;

        var medicationRequest = new MedicationRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FhirId = root.GetProperty("id").GetString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FhirDataJson = fhirJson,
            MedicationCode = string.Empty,
            CodeSystem = string.Empty,
            MedicationDisplay = string.Empty,
            Status = string.Empty
        };

        // Extract patient reference
        if (root.TryGetProperty("subject", out var subject) && 
            subject.TryGetProperty("reference", out var reference))
        {
            var refString = reference.GetString();
            if (refString != null && refString.StartsWith("Patient/"))
            {
                var fhirPatientId = refString.Replace("Patient/", "");
                var patient = await GetPatientByFhirIdAsync(fhirPatientId, tenantId, cancellationToken);
                if (patient != null)
                {
                    medicationRequest.PatientId = patient.Id;
                }
            }
        }

        // Extract medication (RxNorm or other)
        if (root.TryGetProperty("medicationCodeableConcept", out var medication) && 
            medication.TryGetProperty("coding", out var codingArray) && 
            codingArray.GetArrayLength() > 0)
        {
            var coding = codingArray[0];
            if (coding.TryGetProperty("system", out var system))
            {
                medicationRequest.CodeSystem = system.GetString() ?? "";
            }
            if (coding.TryGetProperty("code", out var code))
            {
                medicationRequest.MedicationCode = code.GetString() ?? "";
            }
            if (coding.TryGetProperty("display", out var display))
            {
                medicationRequest.MedicationDisplay = display.GetString() ?? "";
            }
        }

        // Extract status
        if (root.TryGetProperty("status", out var status))
        {
            medicationRequest.Status = status.GetString() ?? "";
        }

        // Extract dosage instructions
        if (root.TryGetProperty("dosageInstruction", out var dosageArray) && dosageArray.GetArrayLength() > 0)
        {
            var dosage = dosageArray[0];
            if (dosage.TryGetProperty("text", out var text))
            {
                medicationRequest.DosageInstructions = text.GetString();
            }
        }

        // Extract authored on
        if (root.TryGetProperty("authoredOn", out var authoredOn))
        {
            if (DateTime.TryParse(authoredOn.GetString(), out var date))
            {
                medicationRequest.AuthoredOn = date;
            }
        }

        _context.MedicationRequests.Add(medicationRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return medicationRequest;
    }

    public async Task<Observation> ImportObservationAsync(string fhirJson, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var fhirData = JsonDocument.Parse(fhirJson);
        var root = fhirData.RootElement;

        var observation = new Observation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FhirId = root.GetProperty("id").GetString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FhirDataJson = fhirJson,
            Code = string.Empty,
            CodeSystem = string.Empty,
            Display = string.Empty,
            Status = string.Empty
        };

        // Extract patient reference
        if (root.TryGetProperty("subject", out var subject) && 
            subject.TryGetProperty("reference", out var reference))
        {
            var refString = reference.GetString();
            if (refString != null && refString.StartsWith("Patient/"))
            {
                var fhirPatientId = refString.Replace("Patient/", "");
                var patient = await GetPatientByFhirIdAsync(fhirPatientId, tenantId, cancellationToken);
                if (patient != null)
                {
                    observation.PatientId = patient.Id;
                }
            }
        }

        // Extract code (LOINC or other)
        if (root.TryGetProperty("code", out var code) && 
            code.TryGetProperty("coding", out var codingArray) && 
            codingArray.GetArrayLength() > 0)
        {
            var coding = codingArray[0];
            if (coding.TryGetProperty("system", out var system))
            {
                observation.CodeSystem = system.GetString() ?? "";
            }
            if (coding.TryGetProperty("code", out var codeValue))
            {
                observation.Code = codeValue.GetString() ?? "";
            }
            if (coding.TryGetProperty("display", out var display))
            {
                observation.Display = display.GetString() ?? "";
            }
        }

        // Extract value
        if (root.TryGetProperty("valueQuantity", out var valueQuantity))
        {
            var valueStr = "";
            if (valueQuantity.TryGetProperty("value", out var value))
            {
                valueStr = value.GetDouble().ToString();
            }
            if (valueQuantity.TryGetProperty("unit", out var unit))
            {
                observation.Unit = unit.GetString();
                valueStr += " " + unit.GetString();
            }
            observation.Value = valueStr;
        }
        else if (root.TryGetProperty("valueString", out var valueString))
        {
            observation.Value = valueString.GetString();
        }

        // Extract status
        if (root.TryGetProperty("status", out var status))
        {
            observation.Status = status.GetString() ?? "";
        }

        // Extract effective date time
        if (root.TryGetProperty("effectiveDateTime", out var effectiveDateTime))
        {
            if (DateTime.TryParse(effectiveDateTime.GetString(), out var date))
            {
                observation.EffectiveDateTime = date;
            }
        }

        _context.Observations.Add(observation);
        await _context.SaveChangesAsync(cancellationToken);

        return observation;
    }

    public async Task<(int Success, int Failed)> ImportBundleAsync(string bundleJson, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var fhirData = JsonDocument.Parse(bundleJson);
        var root = fhirData.RootElement;

        int success = 0;
        int failed = 0;

        if (root.TryGetProperty("entry", out var entries))
        {
            foreach (var entry in entries.EnumerateArray())
            {
                if (entry.TryGetProperty("resource", out var resource))
                {
                    try
                    {
                        var resourceJson = resource.GetRawText();
                        
                        if (resource.TryGetProperty("resourceType", out var resourceType))
                        {
                            var type = resourceType.GetString();
                            
                            switch (type)
                            {
                                case "Patient":
                                    await ImportPatientAsync(resourceJson, tenantId, cancellationToken);
                                    break;
                                case "Condition":
                                    await ImportConditionAsync(resourceJson, tenantId, cancellationToken);
                                    break;
                                case "MedicationRequest":
                                    await ImportMedicationRequestAsync(resourceJson, tenantId, cancellationToken);
                                    break;
                                case "Observation":
                                    await ImportObservationAsync(resourceJson, tenantId, cancellationToken);
                                    break;
                                default:
                                    // Skip unsupported resource types
                                    continue;
                            }
                            
                            success++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error and track which resource failed
                        // In production, use proper logging framework
                        System.Diagnostics.Debug.WriteLine($"Failed to import FHIR resource: {ex.Message}");
                        failed++;
                    }
                }
            }
        }

        return (success, failed);
    }

    public async Task<Patient?> GetPatientByFhirIdAsync(string fhirId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Patients
            .Where(p => p.FhirId == fhirId && p.TenantId == tenantId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
