using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetSymbolTrades;

public sealed class GetSymbolTradesEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var expected = new GetSymbolTradesResponse(0m, 0m, 0m, 0m, 0m, 0, []);

        _bus.InvokeAsync<GetSymbolTradesResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetSymbolTradesEndpoint.Handle(
            symbolId,
            _bus,
            TimeFrame.AllTime,
            100,
            ct
        );

        // Assert
        result.ShouldBeOfType<Ok<GetSymbolTradesResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetSymbolTradesResponse>(Arg.Any<GetSymbolTradesInput>(), ct);
    }
}
