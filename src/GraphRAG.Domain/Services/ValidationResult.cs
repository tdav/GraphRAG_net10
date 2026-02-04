namespace GraphRAG.Domain.Services;

/// <summary>
/// Simple validation result for domain validations.
/// </summary>
public sealed record ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failed(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors
    };
}
