using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.GetSymbolSignals;

public sealed class GetSymbolSignalsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var input = new GetSymbolSignalsInput(symbolId);
        var expected = new GetSymbolSignalsResponse(
            symbolId,
            "Symbol",
            [],
            new SignalSummary(0m, 0, 0, 0, false, false)
        );

        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetSymbolSignalsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetSymbolSignalsResponse>>();
        await _bus.Received(1).InvokeAsync<GetSymbolSignalsResponse>(input, ct);
    }
}
