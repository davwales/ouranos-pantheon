using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class TextMessagePartTests
{
    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<MessagePart>(Guid.NewGuid().ToString());

        // Act
        var part = TextMessagePart.Create(id, "Hello");

        // Assert
        part.Id.ShouldBe(id);
        part.Content.ShouldBe("Hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WhenContentIsEmpty_ShouldThrowArgumentException(string? content)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            TextMessagePart.Create(new Id<MessagePart>(Guid.NewGuid().ToString()), content!)
        );
    }
}
