using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Images.GetImage;

public sealed class GetImageHandlerTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();
    private readonly IFlags _flags = Substitute.For<IFlags>();
    private readonly GetImageHandler _handler;

    public GetImageHandlerTests()
    {
        var flagsmith = Substitute.For<IFlagsmithClient>();
        flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(_flags));
        SetPublicMode(false);

        _handler = new GetImageHandler(
            Substitute.For<ILogger<GetImageHandler>>(),
            _dbContext,
            flagsmith
        );
    }

    private void SetPublicMode(bool isEnabled)
    {
        _flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(isEnabled));
    }

    private async Task<GeneratedImage> SeedImageAsync()
    {
        var image = GeneratedImage.Create(
            new Id<GeneratedImage>(Guid.NewGuid().ToString()),
            [1, 2, 3],
            "image/webp",
            64,
            32,
            "A fox",
            "flux"
        );
        await _dbContext.SeedData(image);
        return image;
    }

    private async Task SeedConversationAsync(
        GeneratedImage image,
        bool isPublic,
        Folder? folder = null
    )
    {
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());

        var message = Message.Create(
            new Id<Message>(Guid.NewGuid().ToString()),
            conversationId,
            "Here it is.",
            Role.Assistant,
            0,
            [
                ImageMessagePart.Create(
                    new Id<MessagePart>(Guid.NewGuid().ToString()),
                    image.Id,
                    image.Width,
                    image.Height,
                    image.Prompt
                ),
            ]
        );

        if (folder is not null)
        {
            await _dbContext.SeedData(folder);
        }

        await _dbContext.SeedData(
            Conversation.Create(
                conversationId,
                new Id<Persona>(Guid.NewGuid().ToString()),
                new Id<ModelConfig>(Guid.NewGuid().ToString()),
                [message],
                [],
                isPublic: isPublic,
                folderId: folder?.Id
            )
        );
    }

    [Fact]
    public async Task Handle_WhenImageExists_ShouldReturnItsContentAndMediaType()
    {
        // Arrange
        var image = await SeedImageAsync();

        // Act
        var result = await _handler.Handle(new GetImageInput(image.Id), CancellationToken.None);

        // Assert
        result.Content.ShouldBe(new byte[] { 1, 2, 3 });
        result.MediaType.ShouldBe("image/webp");
    }

    [Fact]
    public async Task Handle_WhenImageIsMissing_ShouldThrowNotFoundException()
    {
        // Act
        var act = () =>
            _handler.Handle(
                new GetImageInput(new Id<GeneratedImage>(Guid.NewGuid().ToString())),
                CancellationToken.None
            );

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPublicModeAndConversationIsPrivate_ShouldThrowNotFoundException()
    {
        // Arrange
        SetPublicMode(true);
        var image = await SeedImageAsync();
        await SeedConversationAsync(image, isPublic: false);

        // Act
        var act = () => _handler.Handle(new GetImageInput(image.Id), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPublicModeAndFolderIsPrivate_ShouldThrowNotFoundException()
    {
        // Arrange
        SetPublicMode(true);
        var image = await SeedImageAsync();
        var folder = Folder.Create(
            new Id<Folder>(Guid.NewGuid().ToString()),
            "Private",
            isPublic: false
        );
        await SeedConversationAsync(image, isPublic: true, folder);

        // Act
        var act = () => _handler.Handle(new GetImageInput(image.Id), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPublicModeAndConversationIsPublic_ShouldReturnTheImage()
    {
        // Arrange
        SetPublicMode(true);
        var image = await SeedImageAsync();
        await SeedConversationAsync(image, isPublic: true);

        // Act
        var result = await _handler.Handle(new GetImageInput(image.Id), CancellationToken.None);

        // Assert
        result.MediaType.ShouldBe("image/webp");
    }

    [Fact]
    public async Task Handle_WhenPublicModeAndImageBelongsToAnUnsavedChat_ShouldReturnTheImage()
    {
        // Arrange
        SetPublicMode(true);
        var image = await SeedImageAsync();

        // Act
        var result = await _handler.Handle(new GetImageInput(image.Id), CancellationToken.None);

        // Assert
        result.Content.ShouldBe(new byte[] { 1, 2, 3 });
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = () =>
            _handler.Handle(
                new GetImageInput(new Id<GeneratedImage>(Guid.NewGuid().ToString())),
                cts.Token
            );

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
