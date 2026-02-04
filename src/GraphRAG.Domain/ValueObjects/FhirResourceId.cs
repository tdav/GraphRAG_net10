namespace GraphRAG.Domain.ValueObjects;

/// <summary>
/// Value object representing a FHIR resource identifier.
/// </summary>
public readonly record struct FhirResourceId
{
    public string ResourceType { get; }
    public string Id { get; }

    public string Value => string.IsNullOrWhiteSpace(ResourceType)
        ? Id
        : $"{ResourceType}/{Id}";

    public FhirResourceId(string resourceType, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("FHIR id cannot be empty.", nameof(id));
        }

        ResourceType = resourceType?.Trim() ?? string.Empty;
        Id = id.Trim();
    }

    public static FhirResourceId Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("FHIR id cannot be empty.", nameof(value));
        }

        var parts = value.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2
            ? new FhirResourceId(parts[0], parts[1])
            : new FhirResourceId(string.Empty, parts[0]);
    }

    public static bool TryParse(string value, out FhirResourceId resourceId)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            resourceId = default;
            return false;
        }

        var parts = value.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            resourceId = default;
            return false;
        }

        resourceId = parts.Length == 2
            ? new FhirResourceId(parts[0], parts[1])
            : new FhirResourceId(string.Empty, parts[0]);
        return true;
    }

    public override string ToString() => Value;
}
