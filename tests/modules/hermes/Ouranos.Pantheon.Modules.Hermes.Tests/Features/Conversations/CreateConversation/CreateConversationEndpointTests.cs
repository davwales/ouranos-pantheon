using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.CreateConversation;

public sealed class CreateConversationEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateConversationInput(
            new Id<Persona>(Guid.NewGuid().ToString()),
            new Id<ModelConfig>(Guid.NewGuid().ToString()),
            [],
            []
        );
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var expected = new CreateConversationResponse(conversationId, "New Conversation");

        _bus.InvokeAsync<CreateConversationResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateConversationEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<CreateConversationResponse>>();
        await _bus.Received(1).InvokeAsync<CreateConversationResponse>(input, ct);
    }
}
