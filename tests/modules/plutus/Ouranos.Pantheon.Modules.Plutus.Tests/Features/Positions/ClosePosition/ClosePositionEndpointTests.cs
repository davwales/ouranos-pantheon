using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.ClosePosition;

public sealed class ClosePositionEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = new Id<Position>(Guid.NewGuid().ToString());
        var body = new ClosePositionBody(PositionStatus.Bought);
        var expected = new ClosePositionResponse(positionId, PositionStatus.Bought);

        _bus.InvokeAsync<ClosePositionResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await ClosePositionEndpoint.Handle(positionId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<ClosePositionResponse>>();
        await _bus.Received(1).InvokeAsync<ClosePositionResponse>(Arg.Any<ClosePositionInput>(), ct);
    }
}
