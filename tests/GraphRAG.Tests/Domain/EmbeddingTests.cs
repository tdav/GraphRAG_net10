using GraphRAG.Domain.Entities.AI;
using Xunit;

namespace GraphRAG.Tests.Domain;

public class EmbeddingTests
{
    [Fact]
    public void Embedding_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var vector = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        var embedding = new Embedding
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Text = "Patient has diabetes",
            Vector = vector,
            Model = "text-embedding-3-large",
            EntityType = "Patient",
            EntityId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, embedding.Id);
        Assert.Equal("Patient has diabetes", embedding.Text);
        Assert.Equal(vector, embedding.Vector);
        Assert.Equal("text-embedding-3-large", embedding.Model);
        Assert.Equal("Patient", embedding.EntityType);
    }

    [Fact]
    public void Embedding_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var embedding = new Embedding();

        // Assert
        Assert.Equal(string.Empty, embedding.Text);
        Assert.Equal(string.Empty, embedding.Model);
        Assert.Null(embedding.Vector);
        Assert.Null(embedding.EntityType);
    }

    [Fact]
    public void Embedding_WithMetadata_StoresDataCorrectly()
    {
        // Arrange
        const string metadata = "{\"source\":\"clinical_notes\",\"confidence\":0.95}";

        // Act
        var embedding = new Embedding
        {
            Text = "Test text",
            Model = "test-model",
            MetadataJson = metadata
        };

        // Assert
        Assert.Equal(metadata, embedding.MetadataJson);
    }
}
