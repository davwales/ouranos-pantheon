using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Assistants.Tools.GenerateImage;

public sealed class GenerateImageToolTests
{
    private static readonly List<ImageLoraRequest> NoLoras = [];

    private static readonly HermesChatContext Context = new(null, [], ConversationImages.Empty);

    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly string _dbName = Guid.NewGuid().ToString();

    private GenerateImageTool CreateTool(HermesImageGenerationOptions? imageOptions = null)
    {
        return new GenerateImageTool(
            Substitute.For<ILogger<GenerateImageTool>>(),
            _mlClient,
            DbContextExtensions.MockFactory<HermesDbContext>(_dbName),
            Options.Create(
                new HermesOptions
                {
                    ImageGeneration = imageOptions ?? new HermesImageGenerationOptions(),
                }
            )
        );
    }

    private void SetupStream(params ImageGenerationEventDto[] events)
    {
        _mlClient
            .StreamImageGenerationAsync(
                Arg.Any<GenerateImageRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_ => ToAsync(events));
    }

    private static async IAsyncEnumerable<ImageGenerationEventDto> ToAsync(
        IEnumerable<ImageGenerationEventDto> events,
        Exception? failWith = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var imageEvent in events)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return imageEvent;
        }

        if (failWith is not null)
        {
            throw failWith;
        }
    }

    [Fact]
    public async Task InvokeAsync_WhenImageIsGenerated_ShouldSaveItAndReturnTheImage()
    {
        // Arrange
        SetupStream(
            new ImageGenerationCompletedDto([1, 2, 3], "image/png", 1216, 832, 42),
            new ImageGenerationDoneDto("flux")
        );
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            Context,
            """{"prompt":"  A red fox  ","orientation":"Landscape"}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeFalse();
        result.Output.ShouldBe(GenerateImageTool.ShownOutput);
        var image = result.Image.ShouldNotBeNull();
        image.Width.ShouldBe(1216);
        image.Height.ShouldBe(832);
        image.AltText.ShouldBe("A red fox");
        image.Url.ShouldBe($"/api/hermes/images/{image.Id}");

        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var saved = dbContext.GeneratedImages.Single();
        saved.Id.Value.ShouldBe(image.Id);
        saved.Content.ShouldBe(new byte[] { 1, 2, 3 });
        saved.Prompt.ShouldBe("A red fox");
        saved.Model.ShouldBe("flux");
        saved.Seed.ShouldBe(42);

        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                ArgExtensions.IsEquivalent(
                    new GenerateImageRequest("A red fox", 1216, 832, null, NoLoras)
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Theory]
    [InlineData(null, 1024, 1024)]
    [InlineData("\"Portrait\"", 832, 1216)]
    public async Task InvokeAsync_WhenOrientationIsGiven_ShouldRequestItsSize(
        string? orientation,
        int width,
        int height
    )
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool(new HermesImageGenerationOptions { Model = "flux" });

        // Act
        await tool.InvokeAsync(
            Context,
            $$"""{"prompt":"A fox","orientation":{{orientation ?? "null"}}}""",
            CancellationToken.None
        );

        // Assert
        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                ArgExtensions.IsEquivalent(
                    new GenerateImageRequest("A fox", width, height, "flux", NoLoras)
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenSizesAreConfigured_ShouldRequestTheConfiguredSize()
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("sd15"));
        var tool = CreateTool(
            new HermesImageGenerationOptions { PortraitSize = new HermesImageSize(512, 768) }
        );

        // Act
        await tool.InvokeAsync(
            Context,
            """{"prompt":"A fox","orientation":"Portrait"}""",
            CancellationToken.None
        );

        // Assert
        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                ArgExtensions.IsEquivalent(
                    new GenerateImageRequest("A fox", 512, 768, null, NoLoras)
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenPromptIsBlank_ShouldFailWithoutCallingOuranosMl()
    {
        // Arrange
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            Context,
            """{"prompt":"   ","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldBe(GenerateImageTool.EmptyPromptOutput);
        _mlClient
            .DidNotReceive()
            .StreamImageGenerationAsync(
                Arg.Any<GenerateImageRequest>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenPromptIsTooLong_ShouldCapIt()
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool(new HermesImageGenerationOptions { MaxPromptLength = 5 });

        // Act
        await tool.InvokeAsync(
            Context,
            """{"prompt":"A red fox","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                Arg.Is<GenerateImageRequest>(r => r.Prompt == "A red"),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenTheCapFallsMidWord_ShouldCapOnTheWordBoundary()
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool(new HermesImageGenerationOptions { MaxPromptLength = 12 });

        // Act
        await tool.InvokeAsync(
            Context,
            """{"prompt":"A red, fluffy fox","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                Arg.Is<GenerateImageRequest>(r => r.Prompt == "A red"),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenLorasAreActive_ShouldSendThemAndPrependTheirTriggerPhrasesToTheRequestPromptOnly()
    {
        // Arrange
        SetupStream(
            new ImageGenerationCompletedDto([1], "image/png", 1024, 1024, null),
            new ImageGenerationDoneDto("flux")
        );

        var tool = CreateTool();

        var context = new HermesChatContext(
            null,
            [new ImageLora("add-detail", 0.8f, "detailed"), new ImageLora("pixel-art", 1f)],
            ConversationImages.Empty
        );

        // Act
        var result = await tool.InvokeAsync(
            context,
            """{"prompt":"A fox","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                ArgExtensions.IsEquivalent(
                    new GenerateImageRequest(
                        "detailed, A fox",
                        1024,
                        1024,
                        null,
                        new List<ImageLoraRequest> { new("add-detail", 0.8f), new("pixel-art", 1f) }
                    )
                ),
                Arg.Any<CancellationToken>()
            );

        result.Image.ShouldNotBeNull().AltText.ShouldBe("A fox");
        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        dbContext.GeneratedImages.Single().Prompt.ShouldBe("A fox");
    }

    [Fact]
    public async Task InvokeAsync_WhenPromptIsTooLongWithLoras_ShouldCapOnlyThePrompt()
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool(new HermesImageGenerationOptions { MaxPromptLength = 5 });
        var context = new HermesChatContext(
            null,
            [new ImageLora("detail", -0.25f, "fine detail")],
            ConversationImages.Empty
        );

        // Act
        await tool.InvokeAsync(
            context,
            """{"prompt":"A red fox","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                Arg.Is<GenerateImageRequest>(r => r.Prompt == "fine detail, A red"),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenOuranosMlFails_ShouldReturnItsReasonAndSaveNothing()
    {
        // Arrange
        _mlClient
            .StreamImageGenerationAsync(
                Arg.Any<GenerateImageRequest>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_ =>
                ToAsync(
                    [],
                    new OuranosMlRequestException(
                        "not_configured",
                        "SwarmUI is not configured.",
                        HttpStatusCode.ServiceUnavailable
                    )
                )
            );
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            Context,
            """{"prompt":"A fox","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldBe("No image generated: SwarmUI is not configured.");
        result.Image.ShouldBeNull();
        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        dbContext.GeneratedImages.ShouldBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WhenStreamHasNoImage_ShouldFail()
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            Context,
            """{"prompt":"A fox","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldBe(GenerateImageTool.NoImageOutput);
    }

    [Fact]
    public async Task InvokeAsync_WhenCancelled_ShouldThrowAndSaveNothing()
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () =>
            await tool.InvokeAsync(Context, """{"prompt":"A fox","orientation":null}""", cts.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        dbContext.GeneratedImages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAvailable_ShouldFollowTheEnabledOption(bool isEnabled)
    {
        // Arrange
        var tool = CreateTool(new HermesImageGenerationOptions { IsEnabled = isEnabled });

        // Act & Assert
        tool.IsAvailable(Context).ShouldBe(isEnabled);
    }

    [Fact]
    public void Tool_ShouldDeliverAtMostOneImagePerReply()
    {
        // Arrange & Act & Assert
        CreateTool().IsSingleDeliveryPerReply.ShouldBeTrue();
    }

    [Fact]
    public void ParametersSchema_ShouldDescribeThePrompt()
    {
        // Arrange
        var tool = CreateTool();

        // Act
        using var schema = JsonDocument.Parse(tool.ParametersSchema.ToString());

        // Assert
        schema
            .RootElement.GetProperty("properties")
            .GetProperty("prompt")
            .GetProperty("description")
            .GetString()
            .ShouldStartWith("A detailed visual description");
    }

    [Fact]
    public async Task InvokeAsync_WhenThePromptAlreadyHasATriggerPhrase_ShouldNotRepeatIt()
    {
        // Arrange
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool();
        var context = new HermesChatContext(
            null,
            [new ImageLora("pixel-art", 1f, "Pixel Art")],
            ConversationImages.Empty
        );

        // Act
        await tool.InvokeAsync(
            context,
            """{"prompt":"pixel art of a fox","orientation":null}""",
            CancellationToken.None
        );

        // Assert
        _mlClient
            .Received(1)
            .StreamImageGenerationAsync(
                Arg.Is<GenerateImageRequest>(r => r.Prompt == "pixel art of a fox"),
                Arg.Any<CancellationToken>()
            );
    }
}
