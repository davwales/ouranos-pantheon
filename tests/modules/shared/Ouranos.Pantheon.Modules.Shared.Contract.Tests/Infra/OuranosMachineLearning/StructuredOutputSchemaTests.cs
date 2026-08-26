using System.Text.Json;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Infra.OuranosMachineLearning;

public sealed class StructuredOutputSchemaTests
{
    [Fact]
    public void For_WhenTypeHasNestedObjects_ShouldExportStrictSchema()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDocument>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var root = document.RootElement;

        // Assert
        root.GetProperty("type").GetString().ShouldBe("object");
        root.GetProperty("additionalProperties").GetBoolean().ShouldBeFalse();

        var required = root.GetProperty("required")
            .EnumerateArray()
            .Select(node => node.GetString())
            .ToArray();
        required.ShouldBe(["title", "description", "ingredients", "steps"]);

        var ingredients = root.GetProperty("properties").GetProperty("ingredients");
        ingredients.GetProperty("type").GetString().ShouldBe("array");
        ingredients
            .GetProperty("items")
            .GetProperty("additionalProperties")
            .GetBoolean()
            .ShouldBeFalse();

        var itemRequired = ingredients
            .GetProperty("items")
            .GetProperty("required")
            .EnumerateArray()
            .Select(node => node.GetString())
            .ToArray();
        itemRequired.ShouldBe(["quantity", "name"]);
    }

    [Fact]
    public void For_WhenPropertyIsNullable_ShouldExportNullUnionType()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDocument>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var description = document.RootElement.GetProperty("properties").GetProperty("description");

        // Assert
        var types = description
            .GetProperty("type")
            .EnumerateArray()
            .Select(node => node.GetString())
            .ToArray();
        types.ShouldBe(["string", "null"]);
    }

    [Fact]
    public void For_WhenCalledTwice_ShouldReturnCachedInstance()
    {
        // Arrange
        var first = StructuredOutputSchema.For<TestDocument>();
        var second = StructuredOutputSchema.For<TestDocument>();

        // Act & Assert
        ReferenceEquals(first, second).ShouldBeTrue();
    }
}
