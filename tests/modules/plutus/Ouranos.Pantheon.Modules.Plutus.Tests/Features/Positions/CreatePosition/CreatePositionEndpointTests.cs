using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.CreatePosition;

public sealed class CreatePositionEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var body = new CreatePositionBody(
            PositionSide.Buy,
            new Id<Market>(Guid.NewGuid().ToString()),
            new Id<Symbol>(Guid.NewGuid().ToString()),
            150.50m,
            10m
        );
        var expected = new IdResponse<Position>(new Id<Position>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<Position>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreatePositionEndpoint.Handle(body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<IdResponse<Position>>>();
        await _bus.Received(1)
            .InvokeAsync<IdResponse<Position>>(Arg.Any<CreatePositionInput>(), ct);
    }
}
