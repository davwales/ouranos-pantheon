using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Infra.OuranosMachineLearning;

public sealed class StructuredCompletionParserTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();

    private const string ValidJson =
        """{"title":"Chocolate Cake","description":"Rich.","ingredients":[{"quantity":2,"name":"flour"}],"steps":["Mix.","Bake."]}""";

    [Fact]
    public void Parse_WhenContentIsValidJson_ShouldReturnTypedValue()
    {
        // Arrange & Act
        var result = StructuredCompletionParser.Parse<TestDocument>(ValidJson, _logger);

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
        var result = StructuredCompletionParser.Parse<TestDocument>(content, _logger);

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
        var result = StructuredCompletionParser.Parse<TestDocument>(content, _logger);

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
        var result = StructuredCompletionParser.Parse<TestDocument>(content, _logger);

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
        var result = StructuredCompletionParser.Parse<TestDocument>(content, _logger);

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
        var result = StructuredCompletionParser.Parse<TestDocument>(content, _logger);

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
        var result = StructuredCompletionParser.Parse<TestDocument>(content, _logger);

        // Assert
        var parsed = result.ShouldBeOfType<TestDocument>();
        parsed.Title.ShouldBe("Known");
    }

    [Fact]
    public void Parse_WhenContentIsBlank_ShouldReturnNull()
    {
        // Arrange & Act
        var result = StructuredCompletionParser.Parse<TestDocument>("   ", _logger);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Parse_WhenContentIsNull_ShouldReturnNull()
    {
        // Arrange & Act
        var result = StructuredCompletionParser.Parse<TestDocument>(null, _logger);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Parse_WhenContentIsInvalidJson_ShouldReturnNull()
    {
        // Arrange
        const string content = """{"title": "Chocolate Cake" ,,, }""";

        // Act
        var result = StructuredCompletionParser.Parse<TestDocument>(content, _logger);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Parse_WhenPropertyIsAnId_ShouldReadItFromAString()
    {
        // Act
        var result = StructuredCompletionParser.Parse<TestIdArguments>(
            """{"ingredientId":"abc","parentId":null}""",
            _logger
        );

        // Assert
        var parsed = result.ShouldBeOfType<TestIdArguments>();
        parsed.IngredientId.Value.ShouldBe("abc");
        parsed.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Parse_WhenContentIsInvalidJson_ShouldLogAWarning()
    {
        // Arrange
        const string content = """{"title": "Chocolate Cake" ,,, }""";

        // Act
        StructuredCompletionParser.Parse<TestDocument>(content, _logger);

        // Assert
        _logger
            .ReceivedCalls()
            .Count(call =>
                call.GetMethodInfo().Name == nameof(ILogger.Log)
                && (LogLevel?)call.GetArguments()[0] == LogLevel.Warning
                && call.GetArguments()[3] is JsonException
            )
            .ShouldBe(1);
    }
}
