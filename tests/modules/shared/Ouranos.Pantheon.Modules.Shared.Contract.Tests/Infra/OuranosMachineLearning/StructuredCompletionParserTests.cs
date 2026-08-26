using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Infra.OuranosMachineLearning;

public sealed class StructuredCompletionParserTests
{
    private const string ValidJson =
        """{"title":"Chocolate Cake","description":"Rich.","ingredients":[{"quantity":2,"name":"flour"}],"steps":["Mix.","Bake."]}""";

    [Fact]
    public void Parse_WhenContentIsValidJson_ShouldReturnTypedValue()
    {
        // Arrange & Act
        var result = StructuredCompletionParser.Parse<TestDocument>(ValidJson);

        // Assert
        var parsed = result.ShouldBeOfType<TestDocument>();
        parsed.Title.ShouldBe("Chocolate Cake");
        parsed.Description.ShouldBe("Rich.");
        parsed.Ingredients.Count.ShouldBe(1);
        parsed.Ingredients[0].Quantity.ShouldBe(2m);
        parsed.Ingredients[0].Name.ShouldBe("flour");
        parsed.Steps.ShouldBe(["Mix.", "Bake."]);
    }

    [Fact]
    public void Parse_WhenContentIsFencedWithCodeFence_ShouldUnwrapAndParse()
    {
        // Arrange
        var content = $"```json\n{ValidJson}\n```";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content);

        // Assert
        result.ShouldNotBeNull();
        result!.Title.ShouldBe("Chocolate Cake");
    }

    [Fact]
    public void Parse_WhenContentHasTrailingCommas_ShouldStillParse()
    {
        // Arrange
        var content =
            """{"title":"Loose","description":null,"ingredients":[{"quantity":1,"name":"egg",},],"steps":["Bake.",]}""";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content);

        // Assert
        var parsed = result.ShouldBeOfType<TestDocument>();
        parsed.Title.ShouldBe("Loose");
        parsed.Ingredients[0].Name.ShouldBe("egg");
    }

    [Fact]
    public void Parse_WhenContentHasComments_ShouldStillParse()
    {
        // Arrange
        var content =
            "{\"title\":\"Commented\",\"description\":null,// note\n\"ingredients\":[{\"quantity\":1,\"name\":\"egg\"}],\"steps\":[\"Bake.\"]}";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content);

        // Assert
        var parsed = result.ShouldBeOfType<TestDocument>();
        parsed.Title.ShouldBe("Commented");
    }

    [Fact]
    public void Parse_WhenPropertyNamesDifferOnlyByCase_ShouldStillParse()
    {
        // Arrange
        var content =
            """{"Title":"Casey","Description":null,"Ingredients":[{"Quantity":1,"Name":"egg"}],"Steps":["Bake."]}""";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content);

        // Assert
        var parsed = result.ShouldBeOfType<TestDocument>();
        parsed.Title.ShouldBe("Casey");
        parsed.Ingredients[0].Name.ShouldBe("egg");
    }

    [Fact]
    public void Parse_WhenPropertyIsMissing_ShouldUseDefaultValue()
    {
        // Arrange
        var content =
            """{"description":null,"ingredients":[{"quantity":1,"name":"egg"}],"steps":["Bake."]}""";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content);

        // Assert
        var parsed = result.ShouldBeOfType<TestDocument>();
        parsed.Title.ShouldBe(string.Empty);
    }

    [Fact]
    public void Parse_WhenContentHasUnknownProperties_ShouldIgnoreThem()
    {
        // Arrange
        var content =
            """{"title":"Known","description":null,"ingredients":[],"steps":[],"extra":"ignored"}""";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content);

        // Assert
        var parsed = result.ShouldBeOfType<TestDocument>();
        parsed.Title.ShouldBe("Known");
    }

    [Fact]
    public void Parse_WhenContentIsBlank_ShouldReturnNull()
    {
        // Arrange & Act
        var result = StructuredCompletionParser.Parse<TestDocument>("   ");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Parse_WhenContentIsNull_ShouldReturnNull()
    {
        // Arrange & Act
        var result = StructuredCompletionParser.Parse<TestDocument>(null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Parse_WhenContentIsInvalidJson_ShouldReturnNull()
    {
        // Arrange
        const string content = """{"title": "Chocolate Cake" ,,, }""";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content);

        // Assert
        result.ShouldBeNull();
    }
}
