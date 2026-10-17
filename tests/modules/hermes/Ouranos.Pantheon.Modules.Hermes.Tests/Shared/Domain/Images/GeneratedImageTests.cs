using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Images;

public sealed class GeneratedImageTests
{
    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<GeneratedImage>(Guid.NewGuid().ToString());

        // Act
        var image = GeneratedImage.Create(id, [1, 2], "image/png", 64, 32, "A fox", "flux", 7);

        // Assert
        image.Id.ShouldBe(id);
        image.Content.ShouldBe(new byte[] { 1, 2 });
        image.MediaType.ShouldBe("image/png");
        image.Width.ShouldBe(64);
        image.Height.ShouldBe(32);
        image.Prompt.ShouldBe("A fox");
        image.Model.ShouldBe("flux");
        image.Seed.ShouldBe(7);
    }

    [Fact]
    public void Create_WhenContentIsEmpty_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            GeneratedImage.Create(
                new Id<GeneratedImage>(Guid.NewGuid().ToString()),
                [],
                "image/png",
                64,
                32,
                "A fox",
                "flux"
            )
        );
    }

    [Theory]
    [InlineData(0, 32)]
    [InlineData(64, -1)]
    public void Create_WhenSizeIsNotPositive_ShouldThrowArgumentException(int width, int height)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            GeneratedImage.Create(
                new Id<GeneratedImage>(Guid.NewGuid().ToString()),
                [1],
                "image/png",
                width,
                height,
                "A fox",
                "flux"
            )
        );
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    public void Create_WhenPromptIsBlank_ShouldThrowArgumentException(string prompt)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            GeneratedImage.Create(
                new Id<GeneratedImage>(Guid.NewGuid().ToString()),
                [1],
                "image/png",
                64,
                32,
                prompt,
                "flux"
            )
        );
    }
}
