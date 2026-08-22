using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetMarketOverview;

public sealed class GetMarketOverviewEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var expected = new GetMarketOverviewResponse(0m, 0m, 0m, 0, []);

        _bus.InvokeAsync<GetMarketOverviewResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetMarketOverviewEndpoint.Handle(
            marketId,
            _bus,
            TimeFrame.AllTime,
            100,
            ct
        );

        // Assert
        result.ShouldBeOfType<Ok<GetMarketOverviewResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetMarketOverviewResponse>(Arg.Any<GetMarketOverviewInput>(), ct);
    }
}
