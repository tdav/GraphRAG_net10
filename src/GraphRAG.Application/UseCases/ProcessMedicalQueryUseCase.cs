using FluentValidation;
using GraphRAG.Application.DTOs;
using GraphRAG.Application.Interfaces;
using GraphRAG.Application.UseCases.Interfaces;
using Microsoft.Extensions.Logging;

namespace GraphRAG.Application.UseCases;

public class ProcessMedicalQueryUseCase : IProcessMedicalQueryUseCase
{
    private readonly IGraphRagService _graphRagService;
    private readonly IValidator<QueryRequest> _validator;
    private readonly ILogger<ProcessMedicalQueryUseCase> _logger;

    public ProcessMedicalQueryUseCase(
        IGraphRagService graphRagService,
        IValidator<QueryRequest> validator,
        ILogger<ProcessMedicalQueryUseCase> logger)
    {
        _graphRagService = graphRagService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<QueryResponse> ExecuteAsync(QueryRequest request, Guid tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing ProcessMedicalQueryUseCase for tenant {TenantId}", tenantId);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await _graphRagService.ProcessQueryAsync(request, tenantId, cancellationToken);
    }
}