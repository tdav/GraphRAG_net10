using GraphRAG.Application.Interfaces;
using GraphRAG.Domain.Interfaces;
using GraphRAG.Domain.Entities.Core;
using GraphRAG.Infrastructure.Services;
using GraphRAG.Infrastructure.Repositories;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace GraphRAG.Tests.Infrastructure;

public class EtlPipelineIntegrationTests : IntegrationTestBase
{
    private readonly IFhirRepository _fhirRepository;
    private readonly IGraphRepository _graphRepository;
    private readonly IVectorRepository _vectorRepository;
    private readonly IFhirMappingService _mappingService;
    private readonly IAIService _aiService;
    private readonly FhirEtlService _etlService;

    public EtlPipelineIntegrationTests()
    {
        _fhirRepository = new FhirRepository(_context);
        _graphRepository = Substitute.For<IGraphRepository>();
        _vectorRepository = Substitute.For<IVectorRepository>();
        _mappingService = new FhirMappingService();
        _aiService = Substitute.For<IAIService>();
        
        _etlService = new FhirEtlService(
            _fhirRepository,
            _graphRepository,
            _vectorRepository,
            _mappingService,
            _aiService,
            NullLogger<FhirEtlService>.Instance);
            
        _aiService.GenerateEmbeddingAsync(Arg.Any<string>()).Returns(Task.FromResult(new float[1536]));
    }

    [Fact]
    public async Task ProcessBundleAsync_WithValidBundle_ImportsData()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "ETL Test Tenant" };
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var bundleJson = @"
        {
            ""resourceType"": ""Bundle"",
            ""type"": ""collection"",
            ""entry"": [
                {
                    ""resource"": {
                        ""resourceType"": ""Patient"",
                        ""id"": ""pat1"",
                        ""name"": [{ ""family"": ""Doe"", ""given"": [""John""] }]
                    }
                },
                {
                    ""resource"": {
                        ""resourceType"": ""Condition"",
                        ""id"": ""cond1"",
                        ""subject"": { ""reference"": ""Patient/pat1"" },
                        ""code"": { ""coding"": [{ ""system"": ""http://snomed.info/sct"", ""code"": ""38341003"", ""display"": ""Hypertension"" }] }
                    }
                }
            ]
        }";

        // Act
        var result = await _etlService.ProcessBundleAsync(bundleJson, tenantId);

        // Assert
        Assert.Equal(2, result.Success);
        Assert.Equal(0, result.Failed);
        
        var patient = _context.Patients.First(p => p.FhirId == "pat1");
        Assert.Equal("John Doe", patient.Name);
        
        var condition = _context.Conditions.First(c => c.FhirId == "cond1");
        Assert.Equal(patient.Id, condition.PatientId);
    }
}
