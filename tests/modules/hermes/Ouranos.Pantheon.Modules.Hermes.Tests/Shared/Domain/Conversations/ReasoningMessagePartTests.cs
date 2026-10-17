using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class ReasoningMessagePartTests
{
    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<MessagePart>(Guid.NewGuid().ToString());

        // Act
        var part = ReasoningMessagePart.Create(id, "Thinking");

        // Assert
        part.Id.ShouldBe(id);
        part.Content.ShouldBe("Thinking");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WhenContentIsEmpty_ShouldThrowArgumentException(string? content)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            ReasoningMessagePart.Create(new Id<MessagePart>(Guid.NewGuid().ToString()), content!)
        );
    }
}
