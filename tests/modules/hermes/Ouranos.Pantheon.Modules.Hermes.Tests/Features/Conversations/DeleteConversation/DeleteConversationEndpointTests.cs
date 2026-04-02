using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.DeleteConversation;

public sealed class DeleteConversationEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var expected = new IdResponse<Conversation>(conversationId);

        _bus.InvokeAsync<IdResponse<Conversation>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteConversationEndpoint.Handle(conversationId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Conversation>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Conversation>>(Arg.Any<DeleteConversationInput>(), ct);
    }
}
