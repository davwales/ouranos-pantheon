using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared.Domain.Conversations;

public sealed class MessageTests
{
    private readonly IFixture _fixture = new Fixture();

    public MessageTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Message>(Guid.NewGuid().ToString());
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var content = _fixture.Create<string>();
        var role = Role.User;
        var sortOrder = _fixture.Create<int>();

        // Act
        var message = Message.Create(id, conversationId, content, role, sortOrder);

        // Assert
        message.Id.ShouldBe(id);
        message.ConversationId.ShouldBe(conversationId);
        message.Content.ShouldBe(content);
        message.Role.ShouldBe(role);
        message.SortOrder.ShouldBe(sortOrder);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidContent_ShouldThrowArgumentException(string? content)
    {
        // Arrange
        var id = new Id<Message>(Guid.NewGuid().ToString());
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());

        // Act
        var create = () => Message.Create(id, conversationId, content!, Role.User, 0);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}
