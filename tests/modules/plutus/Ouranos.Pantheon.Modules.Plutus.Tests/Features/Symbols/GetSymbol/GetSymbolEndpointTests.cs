using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.GetSymbol;

public sealed class GetSymbolEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var expected = new GetSymbolResponse(
            symbolId,
            "CODE",
            null,
            "Name",
            new Id<Market>(Guid.NewGuid().ToString()),
            new AdditionalFields()
        );

        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetSymbolEndpoint.Handle(symbolId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetSymbolResponse>>();
        await _bus.Received(1).InvokeAsync<GetSymbolResponse>(Arg.Any<GetSymbolInput>(), ct);
    }
}
