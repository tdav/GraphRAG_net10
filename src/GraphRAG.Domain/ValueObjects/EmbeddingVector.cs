namespace GraphRAG.Domain.ValueObjects;

/// <summary>
/// Value object representing an embedding vector.
/// </summary>
public readonly record struct EmbeddingVector
{
    public float[] Values { get; }

    public int Dimension => Values.Length;

    public EmbeddingVector(float[] values)
    {
        if (values == null || values.Length == 0)
        {
            throw new ArgumentException("Embedding vector cannot be empty.", nameof(values));
        }

        Values = values;
    }
}
