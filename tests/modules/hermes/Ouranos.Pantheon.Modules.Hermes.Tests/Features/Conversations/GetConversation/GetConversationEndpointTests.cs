using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Personas;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GetConversation;

public sealed class GetConversationEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var conversationId = new Id<Conversation>(Guid.NewGuid().ToString());
        var personaId = new Id<Persona>(Guid.NewGuid().ToString());
        var modelId = new Id<ModelConfig>(Guid.NewGuid().ToString());
        var expected = new GetConversationResponse(
            conversationId,
            "Test Conversation",
            true,
            null,
            new GetConversationPersonaResponse(personaId, "Bot", "A bot", null, null),
            new GetConversationModelResponse(
                modelId,
                "GPT-4",
                "gpt-4",
                "Prompt",
                null,
                null,
                null,
                null
            ),
            [],
            [],
            null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );

        _bus.InvokeAsync<GetConversationResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetConversationEndpoint.Handle(conversationId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetConversationResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetConversationResponse>(Arg.Any<GetConversationInput>(), ct);
    }
}
