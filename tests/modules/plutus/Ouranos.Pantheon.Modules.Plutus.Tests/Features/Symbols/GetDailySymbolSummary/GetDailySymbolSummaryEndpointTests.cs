using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.GetDailySymbolSummary;

public sealed class GetDailySymbolSummaryEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var symbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var expected = new GetDailySymbolSummaryResponse(100m, 90m, 110m, 50m);

        _bus.InvokeAsync<GetDailySymbolSummaryResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetDailySymbolSummaryEndpoint.Handle(symbolId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetDailySymbolSummaryResponse>>();
        await _bus.Received(1).InvokeAsync<GetDailySymbolSummaryResponse>(Arg.Any<GetDailySymbolSummaryInput>(), ct);
    }
}
