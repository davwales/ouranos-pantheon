using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.GetMarket;

public sealed class GetMarketEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var expected = new GetMarketResponse(marketId, "Test Market", new Taxes(null), false, null, null);

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetMarketEndpoint.Handle(marketId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetMarketResponse>>();
        await _bus.Received(1).InvokeAsync<GetMarketResponse>(Arg.Any<GetMarketInput>(), ct);
    }
}
