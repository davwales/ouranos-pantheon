using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe.Extraction;

public sealed class RecipeExtractorTests
{
    private const string TestJsonLd =
        """{"@type":"Recipe","name":"Chocolate Cake","recipeIngredient":["2 cups flour"],"recipeInstructions":["Bake."]}""";

    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly IOptions<HestiaOptions> _options = Options.Create(new HestiaOptions());

    private RecipeExtractor CreateExtractor()
    {
        return new RecipeExtractor(Substitute.For<ILogger<RecipeExtractor>>(), _mlClient, _options);
    }

    private void SetupCompletion(ExtractedRecipe? recipe)
    {
        _mlClient
            .GenerateStructuredChatCompletionAsync<ExtractedRecipe>(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(recipe);
    }

    private static ExtractedRecipe CreateValidRecipe()
    {
        return new ExtractedRecipe(
            "Chocolate Cake",
            "A rich chocolate cake.",
            [new ExtractedIngredient(2, "cup", "flour")],
            ["Mix.", "Bake."]
        );
    }

    [Fact]
    public async Task ExtractAsync_WhenClientReturnsValidRecipe_ShouldReturnIt()
    {
        // Arrange
        var extractor = CreateExtractor();
        var expected = CreateValidRecipe();
        SetupCompletion(expected);

        // Act
        var result = await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        var extracted = result.ShouldBeOfType<ExtractedRecipe>();
        extracted.Title.ShouldBe("Chocolate Cake");
        extracted.Description.ShouldBe("A rich chocolate cake.");
        extracted.Ingredients.Count.ShouldBe(1);
        extracted.Ingredients[0].Quantity.ShouldBe(2m);
        extracted.Ingredients[0].Unit.ShouldBe("cup");
        extracted.Ingredients[0].Name.ShouldBe("flour");
        extracted.Steps.ShouldBe(["Mix.", "Bake."]);
    }

    [Fact]
    public async Task ExtractAsync_WhenClientReturnsValidRecipe_ShouldCallClientWithConfiguredArguments()
    {
        // Arrange
        var extractor = CreateExtractor();
        SetupCompletion(CreateValidRecipe());

        // Act
        await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        await _mlClient
            .Received(1)
            .GenerateStructuredChatCompletionAsync<ExtractedRecipe>(
                _options.Value.RecipeImport.ModelName,
                Arg.Is<List<MessageDto>>(messages =>
                    messages.Count == 2
                    && messages[0].Role == RoleDto.System
                    && messages[1].Role == RoleDto.User
                    && messages[1].Content == TestJsonLd
                ),
                _options.Value.RecipeImport.Temperature,
                _options.Value.RecipeImport.MaxTokens,
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ExtractAsync_WhenClientReturnsNull_ShouldReturnNull()
    {
        // Arrange
        var extractor = CreateExtractor();
        SetupCompletion(null);

        // Act
        var result = await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExtractAsync_WhenCompletionMissingTitle_ShouldReturnNull()
    {
        // Arrange
        var extractor = CreateExtractor();
        SetupCompletion(CreateValidRecipe() with { Title = string.Empty });

        // Act
        var result = await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExtractAsync_WhenCompletionMissingIngredients_ShouldReturnNull()
    {
        // Arrange
        var extractor = CreateExtractor();
        SetupCompletion(CreateValidRecipe() with { Ingredients = [] });

        // Act
        var result = await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExtractAsync_WhenCompletionMissingSteps_ShouldReturnNull()
    {
        // Arrange
        var extractor = CreateExtractor();
        SetupCompletion(CreateValidRecipe() with { Steps = [] });

        // Act
        var result = await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task ExtractAsync_WhenJsonLdIsBlank_ShouldReturnNullWithoutCallingClient()
    {
        // Arrange
        var extractor = CreateExtractor();

        // Act
        var result = await extractor.ExtractAsync("   ", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        await _mlClient
            .DidNotReceiveWithAnyArgs()
            .GenerateStructuredChatCompletionAsync<ExtractedRecipe>(
                default!,
                default!,
                default,
                default,
                default
            );
    }

    [Fact]
    public async Task ExtractAsync_WhenJsonLdExceedsMaxSize_ShouldReturnNullWithoutCallingClient()
    {
        // Arrange
        var extractor = CreateExtractor();
        var oversized = new string('x', 100_001);

        // Act
        var result = await extractor.ExtractAsync(oversized, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        await _mlClient
            .DidNotReceiveWithAnyArgs()
            .GenerateStructuredChatCompletionAsync<ExtractedRecipe>(
                default!,
                default!,
                default,
                default,
                default
            );
    }

    [Fact]
    public async Task ExtractAsync_WhenModelNotConfigured_ShouldReturnNullWithoutCallingClient()
    {
        // Arrange
        var options = Options.Create(
            new HestiaOptions(
                new RecipeImportOptions(ModelName: string.Empty, MaxTokens: 4096, Temperature: 0f)
            )
        );
        var extractor = new RecipeExtractor(
            Substitute.For<ILogger<RecipeExtractor>>(),
            _mlClient,
            options
        );

        // Act
        var result = await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        await _mlClient
            .DidNotReceiveWithAnyArgs()
            .GenerateStructuredChatCompletionAsync<ExtractedRecipe>(
                default!,
                default!,
                default,
                default,
                default
            );
    }

    [Fact]
    public async Task ExtractAsync_WhenClientThrows_ShouldPropagateException()
    {
        // Arrange
        var extractor = CreateExtractor();
        _mlClient
            .GenerateStructuredChatCompletionAsync<ExtractedRecipe>(
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Task.FromException<ExtractedRecipe?>(
                    new HttpRequestException("ML service unavailable")
                )
            );

        // Act
        var act = async () => await extractor.ExtractAsync(TestJsonLd, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<HttpRequestException>();
    }
}
