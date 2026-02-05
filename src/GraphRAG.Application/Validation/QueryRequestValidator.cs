using FluentValidation;
using GraphRAG.Application.DTOs;

namespace GraphRAG.Application.Validation;

public class QueryRequestValidator : AbstractValidator<QueryRequest>
{
    public QueryRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query cannot be empty.")
            .MaximumLength(2000).WithMessage("Query is too long (max 2000 characters).");

        RuleFor(x => x.MaxRelevantNodes)
            .InclusiveBetween(1, 100).WithMessage("MaxRelevantNodes must be between 1 and 100.");
            
        RuleFor(x => x.Context)
            .Must(ctx => ctx != null && ctx.ContainsKey("tenantId"))
            .WithMessage("Context must contain tenantId.");
    }
}
