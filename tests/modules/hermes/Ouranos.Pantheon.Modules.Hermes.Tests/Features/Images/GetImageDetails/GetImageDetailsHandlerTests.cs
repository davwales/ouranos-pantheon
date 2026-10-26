using Ardalis.GuardClauses;
using Flagsmith;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Images.GetImageDetails;

public sealed class GetImageDetailsHandlerTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();
    private readonly IFlags _flags = Substitute.For<IFlags>();
    private readonly GetImageDetailsHandler _handler;

    public GetImageDetailsHandlerTests()
    {
        var flagsmith = Substitute.For<IFlagsmithClient>();
        flagsmith.GetEnvironmentFlags().Returns(Task.FromResult(_flags));
        SetPublicMode(false);

        _handler = new GetImageDetailsHandler(
            Substitute.For<ILogger<GetImageDetailsHandler>>(),
            _dbContext,
            flagsmith
        );
    }

    private void SetPublicMode(bool isEnabled)
    {
        _flags.IsFeatureEnabled(HermesFeatureFlags.PublicMode).Returns(Task.FromResult(isEnabled));
    }

    private async Task<GeneratedImage> SeedImageAsync(Id<GeneratedImage>? sourceImageId = null)
    {
        var image = GeneratedImage.Create(
            new Id<GeneratedImage>(Guid.NewGuid().ToString()),
            [1, 2, 3],
            "image/png",
            64,
            32,
            "A fox at night",
            "flux",
            7,
            sourceImageId
        );
        await _dbContext.SeedData(image);
        return image;
    }

    [Fact]
    public async Task Handle_WhenImageExists_ShouldReturnItsDetailsAndSource()
    {
        // Arrange
        var sourceId = new Id<GeneratedImage>(Guid.NewGuid().ToString());
        var image = await SeedImageAsync(sourceId);

        // Act
        var result = await _handler.Handle(
            new GetImageDetailsInput(image.Id),
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(
            new GetImageDetailsResponse(
                image.Id,
                64,
                32,
                "A fox at night",
                "flux",
                7,
                sourceId,
                image.CreatedAt
            )
        );
    }

    [Fact]
    public async Task Handle_WhenImageIsMissing_ShouldThrowNotFoundException()
    {
        // Act
        var act = () =>
            _handler.Handle(
                new GetImageDetailsInput(new Id<GeneratedImage>(Guid.NewGuid().ToString())),
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
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        await _dbContext.SeedData(
            Conversation.Create(
                conversationId,
                new Id<Persona>(Guid.NewGuid().ToString()),
                new Id<ModelConfig>(Guid.NewGuid().ToString()),
                [
                    Message.Create(
                        new Id<Message>(Guid.NewGuid().ToString()),
                        conversationId,
                        "Here it is.",
                        Role.Assistant,
                        0,
                        [
                            ImageMessagePart.Create(
                                new Id<MessagePart>(Guid.NewGuid().ToString()),
                                image.Id,
                                64,
                                32,
                                image.Prompt
                            ),
                        ]
                    ),
                ],
                [],
                isPublic: false
            )
        );

        // Act
        var act = () => _handler.Handle(new GetImageDetailsInput(image.Id), CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
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
                new GetImageDetailsInput(new Id<GeneratedImage>(Guid.NewGuid().ToString())),
                cts.Token
            );

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
