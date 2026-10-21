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

    [Fact]
    public void For_WhenPropertyIsAnId_ShouldExportString()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestIdArguments>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var properties = document.RootElement.GetProperty("properties");

        // Assert
        properties.GetProperty("ingredientId").GetProperty("type").GetString().ShouldBe("string");
        properties
            .GetProperty("parentId")
            .GetProperty("type")
            .EnumerateArray()
            .Select(node => node.GetString())
            .ToArray()
            .ShouldBe(["string", "null"]);
    }

    [Fact]
    public void For_WhenParameterIsDescribed_ShouldExportItsDescription()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDescribedArguments>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var properties = document.RootElement.GetProperty("properties");

        // Assert
        properties
            .GetProperty("name")
            .GetProperty("description")
            .GetString()
            .ShouldBe("The ingredient name.");
    }

    [Fact]
    public void For_WhenPropertyIsDescribed_ShouldExportItsDescription()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDescribedArguments>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var properties = document.RootElement.GetProperty("properties");

        // Assert
        properties
            .GetProperty("servings")
            .GetProperty("description")
            .GetString()
            .ShouldBe("Ignored by the exporter otherwise.");
    }

    [Fact]
    public void For_WhenPropertyIsNotDescribed_ShouldExportNoDescription()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDescribedArguments>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var properties = document.RootElement.GetProperty("properties");

        // Assert
        properties.GetProperty("note").TryGetProperty("description", out _).ShouldBeFalse();
        document.RootElement.TryGetProperty("description", out _).ShouldBeFalse();
    }

    [Fact]
    public void For_WhenEnumIsDescribed_ShouldExportItsDescription()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDescribedArguments>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var preparation = document.RootElement.GetProperty("properties").GetProperty("preparation");

        // Assert
        preparation
            .GetProperty("description")
            .GetString()
            .ShouldBe("How the ingredient is prepared.");
        preparation.GetProperty("enum").GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public void For_WhenIdIsDescribed_ShouldKeepItsDescription()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDescribedArguments>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var properties = document.RootElement.GetProperty("properties");

        // Assert
        var ingredientId = properties.GetProperty("ingredientId");
        ingredientId.GetProperty("type").GetString().ShouldBe("string");
        ingredientId.GetProperty("description").GetString().ShouldBe("The ingredient to replace.");
        properties
            .GetProperty("parentId")
            .GetProperty("description")
            .GetString()
            .ShouldBe("The parent ingredient, if any.");
    }

    [Fact]
    public void For_WhenArgumentsAreDescribed_ShouldKeepTheStrictShape()
    {
        // Arrange
        var schema = StructuredOutputSchema.For<TestDescribedArguments>().ToString();

        // Act
        using var document = JsonDocument.Parse(schema);
        var root = document.RootElement;

        // Assert
        root.GetProperty("additionalProperties").GetBoolean().ShouldBeFalse();
        root.GetProperty("required")
            .EnumerateArray()
            .Select(node => node.GetString())
            .ToArray()
            .ShouldBe(
                ["servings", "name", "note", "ingredientId", "parentId", "preparation"],
                ignoreOrder: true
            );
    }
}
