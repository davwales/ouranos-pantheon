using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.EditImage;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Assistants.Tools.EditImage;

public sealed class EditImageToolTests
{
    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly string _dbName = Guid.NewGuid().ToString();

    private EditImageTool CreateTool(HermesImageGenerationOptions? imageOptions = null)
    {
        return new EditImageTool(
            Substitute.For<ILogger<EditImageTool>>(),
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

    private async Task<ConversationImage> SaveImageAsync(int imageRef, byte[] content)
    {
        var image = GeneratedImage.Create(
            new Id<GeneratedImage>(Guid.NewGuid().ToString()),
            content,
            "image/png",
            64,
            64,
            $"Prompt {imageRef}",
            "flux"
        );
        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        await dbContext.GeneratedImages.AddAsync(image);
        await dbContext.SaveChangesAsync();
        return new ConversationImage(imageRef, image.Id, image.Prompt);
    }

    private static HermesChatContext ContextWith(
        IReadOnlyList<ConversationImage> images,
        ConversationImage? attached = null,
        IReadOnlyList<ImageLora>? loras = null
    )
    {
        return new HermesChatContext(
            null,
            loras ?? [],
            new ConversationImages(images, attached, images.Count > 0 ? images[^1] : null)
        );
    }

    private void SetupStream(params ImageGenerationEventDto[] events)
    {
        _mlClient
            .StreamImageEditAsync(Arg.Any<EditImageRequest>(), Arg.Any<CancellationToken>())
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

    private EditImageRequest ReceivedRequest()
    {
        var call = _mlClient
            .ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(_mlClient.StreamImageEditAsync));
        return (EditImageRequest)call.GetArguments()[0]!;
    }

    [Fact]
    public async Task InvokeAsync_WhenImageIsEdited_ShouldSaveItWithItsSourceAndReturnIt()
    {
        // Arrange
        var source = await SaveImageAsync(1, [9, 9]);
        SetupStream(
            new ImageGenerationCompletedDto([1, 2, 3], "image/png", 832, 1216, 7),
            new ImageGenerationDoneDto("flux")
        );
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            ContextWith([source]),
            """{"prompt":" A fox at night ","change":"Moderate","image":null}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeFalse();
        result.Output.ShouldBe($"Edited image 1. {EditImageTool.ShownOutput}");
        var image = result.Image.ShouldNotBeNull();
        image.AltText.ShouldBe("A fox at night");
        image.Width.ShouldBe(832);

        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        var saved = dbContext.GeneratedImages.Single(i => i.Id != source.Id);
        saved.Id.Value.ShouldBe(image.Id);
        saved.SourceImageId.ShouldBe(source.Id);
        saved.Prompt.ShouldBe("A fox at night");
        saved.Seed.ShouldBe(7);

        var request = ReceivedRequest();
        request.Prompt.ShouldBe("A fox at night");
        request.Image.ShouldBe(new byte[] { 9, 9 });
        request.Strength.ShouldBe(0.55f);
    }

    [Theory]
    [InlineData("Subtle", 0.1f)]
    [InlineData("Moderate", 0.5f)]
    [InlineData("Strong", 0.9f)]
    public async Task InvokeAsync_WhenChangeIsGiven_ShouldUseItsConfiguredStrength(
        string change,
        float strength
    )
    {
        // Arrange
        var source = await SaveImageAsync(1, [1]);
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool(
            new HermesImageGenerationOptions
            {
                EditStrengths = new HermesImageEditStrengths
                {
                    Subtle = 0.1f,
                    Moderate = 0.5f,
                    Strong = 0.9f,
                },
            }
        );

        // Act
        await tool.InvokeAsync(
            ContextWith([source]),
            $$"""{"prompt":"A fox","change":"{{change}}","image":null}""",
            CancellationToken.None
        );

        // Assert
        ReceivedRequest().Strength.ShouldBe(strength);
    }

    [Fact]
    public async Task InvokeAsync_WhenUserAttachedAnImage_ShouldEditItOverTheNamedOne()
    {
        // Arrange
        var first = await SaveImageAsync(1, [1]);
        var second = await SaveImageAsync(2, [2]);
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool();

        // Act
        await tool.InvokeAsync(
            ContextWith([first, second], attached: first),
            """{"prompt":"A fox","change":"Subtle","image":"image 2"}""",
            CancellationToken.None
        );

        // Assert
        ReceivedRequest().Image.ShouldBe(new byte[] { 1 });
    }

    [Theory]
    [InlineData("image 1")]
    [InlineData("1")]
    [InlineData("Image #1")]
    public async Task InvokeAsync_WhenAnImageIsNamed_ShouldEditIt(string imageRef)
    {
        // Arrange
        var first = await SaveImageAsync(1, [1]);
        var second = await SaveImageAsync(2, [2]);
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool();

        // Act
        await tool.InvokeAsync(
            ContextWith([first, second]),
            $$"""{"prompt":"A fox","change":"Subtle","image":"{{imageRef}}"}""",
            CancellationToken.None
        );

        // Assert
        ReceivedRequest().Image.ShouldBe(new byte[] { 1 });
    }

    [Fact]
    public async Task InvokeAsync_WhenNoImageIsNamed_ShouldEditTheLatest()
    {
        // Arrange
        var first = await SaveImageAsync(1, [1]);
        var second = await SaveImageAsync(2, [2]);
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool();

        // Act
        await tool.InvokeAsync(
            ContextWith([first, second]),
            """{"prompt":"A fox","change":"Subtle"}""",
            CancellationToken.None
        );

        // Assert
        ReceivedRequest().Image.ShouldBe(new byte[] { 2 });
    }

    [Theory]
    [InlineData("image 3")]
    [InlineData("the fox")]
    [InlineData("99999999999999")]
    public async Task InvokeAsync_WhenNamedImageIsUnknown_ShouldFailListingTheImages(
        string imageRef
    )
    {
        // Arrange
        var first = await SaveImageAsync(1, [1]);
        var second = await SaveImageAsync(2, [2]);
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            ContextWith([first, second]),
            $$"""{"prompt":"A fox","change":"Subtle","image":"{{imageRef}}"}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldContain("use one of image 1, image 2");
        _mlClient.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreNoImages_ShouldFail()
    {
        // Arrange
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            ContextWith([]),
            """{"prompt":"A fox","change":"Subtle"}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldBe(EditImageTool.NoSourceOutput);
        _mlClient.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WhenSourceWasPurged_ShouldFail()
    {
        // Arrange
        var purged = new ConversationImage(
            1,
            new Id<GeneratedImage>(Guid.NewGuid().ToString()),
            "A fox"
        );
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            ContextWith([purged]),
            """{"prompt":"A fox","change":"Subtle"}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldBe("No image edited: image 1 is no longer available.");
        _mlClient.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WhenPromptIsBlank_ShouldFailWithoutCallingOuranosMl()
    {
        // Arrange
        var source = await SaveImageAsync(1, [1]);
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            ContextWith([source]),
            """{"prompt":"  ","change":"Subtle"}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldBe(EditImageTool.EmptyPromptOutput);
        _mlClient.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WhenLorasAreActive_ShouldAppendTheirTagsToTheRequestPrompt()
    {
        // Arrange
        var source = await SaveImageAsync(1, [1]);
        SetupStream(new ImageGenerationDoneDto("flux"));
        var tool = CreateTool();

        // Act
        await tool.InvokeAsync(
            ContextWith([source], loras: [new ImageLora("pixel-art", 0.8f)]),
            """{"prompt":"A fox","change":"Subtle"}""",
            CancellationToken.None
        );

        // Assert
        ReceivedRequest().Prompt.ShouldBe("A fox <lora:pixel-art:0.8>");
    }

    [Fact]
    public async Task InvokeAsync_WhenOuranosMlFails_ShouldReturnItsReasonAndSaveNothing()
    {
        // Arrange
        var source = await SaveImageAsync(1, [1]);
        _mlClient
            .StreamImageEditAsync(Arg.Any<EditImageRequest>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
                ToAsync([], new OuranosMlRequestException("invalid_image", "Unreadable image."))
            );
        var tool = CreateTool();

        // Act
        var result = await tool.InvokeAsync(
            ContextWith([source]),
            """{"prompt":"A fox","change":"Subtle"}""",
            CancellationToken.None
        );

        // Assert
        result.IsError.ShouldBeTrue();
        result.Output.ShouldBe("No image generated: Unreadable image.");
        await using var dbContext = DbContextExtensions.Mock<HermesDbContext>(_dbName);
        dbContext.GeneratedImages.Count().ShouldBe(1);
    }

    [Fact]
    public void Tool_ShouldDeliverAtMostOneImagePerReply()
    {
        // Arrange & Act & Assert
        CreateTool().IsSingleDeliveryPerReply.ShouldBeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAvailable_ShouldFollowTheEnabledOption(bool isEnabled)
    {
        // Arrange
        var tool = CreateTool(new HermesImageGenerationOptions { IsEnabled = isEnabled });

        // Act & Assert
        tool.IsAvailable(ContextWith([])).ShouldBe(isEnabled);
    }

    [Fact]
    public void ParametersSchema_ShouldOfferTheNamedChanges()
    {
        // Arrange
        var tool = CreateTool();

        // Act
        using var schema = JsonDocument.Parse(tool.ParametersSchema.ToString());

        // Assert
        var properties = schema.RootElement.GetProperty("properties");
        properties
            .GetProperty("change")
            .GetProperty("enum")
            .EnumerateArray()
            .Select(e => e.GetString())
            .ShouldBe(["Subtle", "Moderate", "Strong"]);
        properties
            .GetProperty("prompt")
            .GetProperty("description")
            .GetString()
            .ShouldContain("whole image");
    }
}
