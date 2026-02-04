using GraphRAG.Domain.ValueObjects;
using Xunit;

namespace GraphRAG.Tests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void FhirResourceId_Parse_SplitsResourceTypeAndId()
    {
        var resourceId = FhirResourceId.Parse("Patient/123");

        Assert.Equal("Patient", resourceId.ResourceType);
        Assert.Equal("123", resourceId.Id);
        Assert.Equal("Patient/123", resourceId.Value);
    }

    [Fact]
    public void FhirResourceId_Parse_HandlesIdOnly()
    {
        var resourceId = FhirResourceId.Parse("abc");

        Assert.Equal(string.Empty, resourceId.ResourceType);
        Assert.Equal("abc", resourceId.Id);
        Assert.Equal("abc", resourceId.Value);
    }

    [Fact]
    public void ConceptCode_ToString_UsesSystemPrefix()
    {
        var code = new ConceptCode("http://snomed.info/sct", "12345", "Test");

        Assert.Equal("http://snomed.info/sct:12345", code.ToString());
        Assert.Equal("Test", code.Display);
    }

    [Fact]
    public void EmbeddingVector_StoresDimension()
    {
        var vector = new EmbeddingVector(new[] { 0.1f, 0.2f, 0.3f });

        Assert.Equal(3, vector.Dimension);
        Assert.Equal(0.2f, vector.Values[1]);
    }
}
