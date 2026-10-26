using Ouranos.Pantheon.Modules.Hermes.Features.Images;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Folders;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Images;

public sealed class ImageVisibilityTests
{
    private readonly HermesDbContext _dbContext = DbContextExtensions.Mock<HermesDbContext>();
    private readonly Id<GeneratedImage> _imageId = new(Guid.NewGuid().ToString());

    private async Task SeedConversationAsync(Role role, bool isPublic, Folder? folder = null)
    {
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());

        var message = Message.Create(
            new Id<Message>(Guid.NewGuid().ToString()),
            conversationId,
            "Here it is.",
            role,
            0,
            [
                ImageMessagePart.Create(
                    new Id<MessagePart>(Guid.NewGuid().ToString()),
                    _imageId,
                    64,
                    64,
                    "A fox"
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

    [Theory]
    [InlineData(Role.Assistant)]
    [InlineData(Role.User)]
    public async Task IsImageInPrivateConversationAsync_WhenAPrivateConversationShowsIt_ShouldBeTrue(
        Role role
    )
    {
        // Arrange
        await SeedConversationAsync(role, isPublic: false);

        // Act
        var result = await _dbContext.IsImageInPrivateConversationAsync(
            _imageId,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsImageInPrivateConversationAsync_WhenItsFolderIsPrivate_ShouldBeTrue()
    {
        // Arrange
        var folder = Folder.Create(
            new Id<Folder>(Guid.NewGuid().ToString()),
            "Private",
            isPublic: false
        );
        await SeedConversationAsync(Role.Assistant, isPublic: true, folder);

        // Act
        var result = await _dbContext.IsImageInPrivateConversationAsync(
            _imageId,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsImageInPrivateConversationAsync_WhenOnlyPublicOrNoConversationsShowIt_ShouldBeFalse()
    {
        // Arrange
        await SeedConversationAsync(Role.Assistant, isPublic: true);

        // Act
        var result = await _dbContext.IsImageInPrivateConversationAsync(
            _imageId,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeFalse();
    }
}
