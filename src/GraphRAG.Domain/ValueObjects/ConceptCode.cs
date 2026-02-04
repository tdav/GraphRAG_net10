namespace GraphRAG.Domain.ValueObjects;

/// <summary>
/// Value object representing a clinical terminology code.
/// </summary>
public readonly record struct ConceptCode
{
    public string System { get; }
    public string Code { get; }
    public string? Display { get; }

    public ConceptCode(string system, string code, string? display = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Concept code cannot be empty.", nameof(code));
        }

        System = system?.Trim() ?? string.Empty;
        Code = code.Trim();
        Display = string.IsNullOrWhiteSpace(display) ? null : display.Trim();
    }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(System))
        {
            return $"{System}:{Code}";
        }

        return Code;
    }
}
