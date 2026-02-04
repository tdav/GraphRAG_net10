using GraphRAG.Domain.Entities.Medical;
using Xunit;

namespace GraphRAG.Tests.Domain;

public class PatientTests
{
    [Fact]
    public void Patient_WithValidData_CreatesSuccessfully()
    {
        // Arrange & Act
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            FhirId = "patient-123",
            Name = "John Doe",
            BirthDate = new DateTime(1980, 1, 1),
            Gender = "male",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, patient.Id);
        Assert.NotEqual(Guid.Empty, patient.TenantId);
        Assert.Equal("patient-123", patient.FhirId);
        Assert.Equal("John Doe", patient.Name);
        Assert.NotNull(patient.BirthDate);
        Assert.Equal("male", patient.Gender);
    }

    [Fact]
    public void Patient_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var patient = new Patient();

        // Assert
        Assert.Equal(string.Empty, patient.FhirId);
        Assert.Equal(string.Empty, patient.Name);
        Assert.Null(patient.BirthDate);
        Assert.Null(patient.Gender);
    }

    [Fact]
    public void Patient_WithFhirJson_StoresDataCorrectly()
    {
        // Arrange
        const string fhirJson = "{\"resourceType\":\"Patient\",\"id\":\"pat-123\"}";

        // Act
        var patient = new Patient
        {
            FhirId = "pat-123",
            Name = "Test Patient",
            FhirDataJson = fhirJson
        };

        // Assert
        Assert.Equal(fhirJson, patient.FhirDataJson);
    }
}
