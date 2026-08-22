using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetMarketTrades;

public sealed class GetMarketTradesEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetMarketTradesInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            TimeFrame.AllTime,
            Take: 10
        );
        var expected = new PagedResponse<GetMarketTradesResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetMarketTradesEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetMarketTradesResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetMarketTradesResponse>>(input, ct);
    }
}
