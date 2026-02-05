using GraphRAG.Application.DTOs;
using GraphRAG.Application.Interfaces;
using GraphRAG.Application.UseCases;
using GraphRAG.Application.UseCases.Interfaces;
using NSubstitute;
using Xunit;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace GraphRAG.Tests.Application;

public class ProcessMedicalQueryUseCaseTests
{
    private readonly IGraphRagService _graphRagService;
    private readonly IExplainReasoningUseCase _explainReasoningUseCase;
    private readonly IValidator<QueryRequest> _validator;
    private readonly ILogger<ProcessMedicalQueryUseCase> _logger;
    private readonly ProcessMedicalQueryUseCase _useCase;

    public ProcessMedicalQueryUseCaseTests()
    {
        _graphRagService = Substitute.For<IGraphRagService>();
        _explainReasoningUseCase = Substitute.For<IExplainReasoningUseCase>();
        _validator = Substitute.For<IValidator<QueryRequest>>();
        _logger = Substitute.For<ILogger<ProcessMedicalQueryUseCase>>();
        _useCase = new ProcessMedicalQueryUseCase(_graphRagService, _explainReasoningUseCase, _validator, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsResponseFromService()
    {
        // Arrange
        var request = new QueryRequest { Query = "What is the patient status?" };
        var tenantId = Guid.NewGuid();
        var expectedResponse = new QueryResponse { Answer = "Patient is stable." };

        _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        _graphRagService.ProcessQueryAsync(request, tenantId, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _useCase.ExecuteAsync(request, tenantId);

        // Assert
        Assert.Equal(expectedResponse.Answer, result.Answer);
        await _graphRagService.Received(1).ProcessQueryAsync(request, tenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new QueryRequest { Query = "" };
        var tenantId = Guid.NewGuid();
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Query", "Query cannot be empty")
        };

        _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _useCase.ExecuteAsync(request, tenantId));
        await _graphRagService.DidNotReceive().ProcessQueryAsync(Arg.Any<QueryRequest>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}