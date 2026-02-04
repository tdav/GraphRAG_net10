using GraphRAG.Domain.Entities.Medical;
using GraphRAG.Infrastructure.Services;
using Xunit;

namespace GraphRAG.Tests.Domain;

public class DomainServiceTests
{
    [Fact]
    public async Task ValidationService_RejectsEmptyPatientName()
    {
        var service = new ValidationService();
        var patient = new Patient { Name = "" };

        var result = await service.ValidatePatientAsync(patient);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidationService_RejectsNullPatient()
    {
        var service = new ValidationService();

        var result = await service.ValidatePatientAsync(null!);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidationService_AcceptsValidQuery()
    {
        var service = new ValidationService();
        var nonEmptyQuery = "test query";

        var result = await service.ValidateQueryAsync(nonEmptyQuery);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidationService_RejectsWhitespaceQuery()
    {
        var service = new ValidationService();

        var result = await service.ValidateQueryAsync("   ");

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task MedicalTerminologyService_NormalizesToSnomed()
    {
        var service = new MedicalTerminologyService();

        var code = await service.NormalizeToSnomedCtAsync("Diabetes");

        Assert.NotNull(code);
        Assert.Equal("http://snomed.info/sct", code?.System);
        Assert.Equal(MedicalTerminologyService.PlaceholderSnomedCode, code?.Code);
        Assert.Equal("Diabetes", code?.Display);
    }
}
