using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.LinkPosition;

public sealed class LinkPositionEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = new Id<Position>(Guid.NewGuid().ToString());
        var body = new LinkPositionBody(new Id<Position>(Guid.NewGuid().ToString()));
        var expected = new IdResponse<Position>(new Id<Position>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<Position>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await LinkPositionEndpoint.Handle(positionId, body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<Position>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<Position>>(Arg.Any<LinkPositionInput>(), ct);
    }
}
