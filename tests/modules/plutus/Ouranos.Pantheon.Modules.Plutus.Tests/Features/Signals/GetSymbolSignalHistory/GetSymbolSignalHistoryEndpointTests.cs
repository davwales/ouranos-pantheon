using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.GetSymbolSignalHistory;

public sealed class GetSymbolSignalHistoryEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var input = new GetSymbolSignalHistoryInput(symbolId);
        var expected = new GetSymbolSignalHistoryResponse(
            symbolId,
            "Symbol",
            [],
            new SignalSummary(0m, 0, 0, 0, false, false)
        );

        _bus.InvokeAsync<GetSymbolSignalHistoryResponse>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetSymbolSignalHistoryEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetSymbolSignalHistoryResponse>>();
        await _bus.Received(1).InvokeAsync<GetSymbolSignalHistoryResponse>(input, ct);
    }
}
