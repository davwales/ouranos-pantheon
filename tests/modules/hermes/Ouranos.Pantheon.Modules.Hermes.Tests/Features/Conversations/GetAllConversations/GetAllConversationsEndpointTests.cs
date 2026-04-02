using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.Conversations.GetAllConversations;

public sealed class GetAllConversationsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllConversationsInput();
        var expected = new List<GetAllConversationsResponse>();

        _bus.InvokeAsync<List<GetAllConversationsResponse>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllConversationsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<List<GetAllConversationsResponse>>>();
        await _bus.Received(1).InvokeAsync<List<GetAllConversationsResponse>>(input, ct);
    }
}
