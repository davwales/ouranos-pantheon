using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode;
using Ouranos.Pantheon.Modules.Hermes.Features.PublicMode.GetPublicMode.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Features.PublicMode.GetPublicMode;

public sealed class GetPublicModeEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new GetPublicModeResponse(true);

        _bus.InvokeAsync<GetPublicModeResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetPublicModeEndpoint.Handle(_bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetPublicModeResponse>>().Value.ShouldBeSameAs(expected);
        await _bus.Received(1).InvokeAsync<GetPublicModeResponse>(new GetPublicModeInput(), ct);
    }
}
