using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class ImageMessagePartTests
{
    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<MessagePart>(Guid.NewGuid().ToString());
        var imageId = new Id<GeneratedImage>(Guid.NewGuid().ToString());

        // Act
        var part = ImageMessagePart.Create(id, imageId, 64, 32, "A fox");

        // Assert
        part.Id.ShouldBe(id);
        part.ImageId.ShouldBe(imageId);
        part.Width.ShouldBe(64);
        part.Height.ShouldBe(32);
        part.AltText.ShouldBe("A fox");
    }

    [Fact]
    public void Create_WhenImageIdIsDefault_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            ImageMessagePart.Create(
                new Id<MessagePart>(Guid.NewGuid().ToString()),
                default,
                64,
                32,
                "A fox"
            )
        );
    }

    [Fact]
    public void Create_WhenSizeIsNotPositive_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            ImageMessagePart.Create(
                new Id<MessagePart>(Guid.NewGuid().ToString()),
                new Id<GeneratedImage>(Guid.NewGuid().ToString()),
                0,
                32,
                "A fox"
            )
        );
    }
}
