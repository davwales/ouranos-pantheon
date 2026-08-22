using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Positions.GetPosition;

public sealed class GetPositionEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var positionId = new Id<Position>(Guid.NewGuid().ToString());
        var expected = new GetPositionResponse(
            positionId,
            PositionSide.Buy,
            PositionStatus.Pending,
            new Id<Market>(Guid.NewGuid().ToString()),
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "Test Symbol",
            150.50m,
            10m,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow
        );

        _bus.InvokeAsync<GetPositionResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetPositionEndpoint.Handle(positionId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetPositionResponse>>();
        await _bus.Received(1).InvokeAsync<GetPositionResponse>(Arg.Any<GetPositionInput>(), ct);
    }
}
