using GraphRAG.Application.Interfaces;
using GraphRAG.Application.UseCases;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.Logging;

namespace GraphRAG.Tests.Application;

public class ImportFhirDataUseCaseTests
{
    private readonly IFhirEtlService _fhirEtlService;
    private readonly ILogger<ImportFhirDataUseCase> _logger;
    private readonly ImportFhirDataUseCase _useCase;

    public ImportFhirDataUseCaseTests()
    {
        _fhirEtlService = Substitute.For<IFhirEtlService>();
        _logger = Substitute.For<ILogger<ImportFhirDataUseCase>>();
        _useCase = new ImportFhirDataUseCase(_fhirEtlService, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceAndReturnsResult()
    {
        // Arrange
        var bundleJson = "{ \"resourceType\": \"Bundle\" }";
        var tenantId = Guid.NewGuid();
        var expectedResult = (10, 2);

        _fhirEtlService.ProcessBundleAsync(bundleJson, tenantId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _useCase.ExecuteAsync(bundleJson, tenantId);

        // Assert
        Assert.Equal(expectedResult, result);
        await _fhirEtlService.Received(1).ProcessBundleAsync(bundleJson, tenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyBundle_LogsWarningAndReturnsZero()
    {
        // Arrange
        var bundleJson = "";
        var tenantId = Guid.NewGuid();

        // Act
        var result = await _useCase.ExecuteAsync(bundleJson, tenantId);

        // Assert
        Assert.Equal((0, 0), result);
        await _fhirEtlService.DidNotReceive().ProcessBundleAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
