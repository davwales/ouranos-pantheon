using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskMarketAnalyst.MarketAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools.FindSymbols;

public sealed class FindSymbolsToolTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly FindSymbolsTool _tool;

    public FindSymbolsToolTests()
    {
        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.View(Factory.TwistedBow(), Factory.RunePlatebody()));

        _tool = new FindSymbolsTool(
            Substitute.For<ILogger<FindSymbolsTool>>(),
            _bus,
            Options.Create(new PlutusOptions())
        );
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Factory.Context(), arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenConditionsAreValid_ShouldScreenTheWholeMarketWithCappedRows()
    {
        // Act
        var result = await InvokeAsync(
            """
            {"conditions":[{"field":"Roi","operator":"Gt","value":"5"}],
             "sortBy":"Roi","descending":true,"take":50}
            """
        );

        // Assert
        var data = result.ShouldContainData<ScreenedSymbolsData>();
        data.TotalMatches.ShouldBe(42);

        data.Symbols.Select(symbol => symbol.SymbolId)
            .ShouldBe([Factory.TwistedBowId, Factory.RunePlatebodyId]);

        data.IgnoredConditions.ShouldBeNull();

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Is<GetMarketTradesInput>(i =>
                    i.Skip == 0
                    && i.Take == new PlutusOptions().AssistantTools.MaxRows
                    && i.SortField == "roi"
                    && i.SortDirection == "desc"
                    && i.Filter != null
                    && i.Filter.SequenceEqual(new[] { "roi:gt:0.05" })
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenAConditionIsInvalid_ShouldReportItAlongsideTheMatches()
    {
        // Act
        var result = await InvokeAsync(
            """
            {"conditions":[{"field":"Margin","operator":"Gt","value":"high"}],
             "sortBy":null,"descending":true,"take":null}
            """
        );

        // Assert
        var data = result.ShouldContainData<ScreenedSymbolsData>();
        data.IgnoredConditions.ShouldBe(["Ignored Margin Gt 'high': not a number."]);
        data.Symbols.Count.ShouldBe(2);

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Is<GetMarketTradesInput>(i =>
                    i.SortField == null
                    && i.SortDirection == null
                    && i.Take == new PlutusOptions().AssistantTools.DefaultRows
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenARowHasNoVolumeOrLowPrice_ShouldOmitAverageAndRoi()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                Factory.View(
                    Factory.TwistedBow() with
                    {
                        SymbolSubcode = "p2p",
                        TotalVolume = 0,
                        MinPrice = 0,
                    }
                )
            );

        // Act
        var result = await InvokeAsync(
            """{"conditions":[],"sortBy":null,"descending":true,"take":null}"""
        );

        // Assert
        var symbol = result.ShouldContainData<ScreenedSymbolsData>().Symbols.Single();
        symbol.Subcode.ShouldBe("p2p");
        symbol.AveragePrice.ShouldBeNull();
        symbol.RoiPercent.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new FindSymbolsTool(
                Substitute.For<ILogger<FindSymbolsTool>>(),
                null!,
                Options.Create(new PlutusOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenOptionsAreNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new FindSymbolsTool(Substitute.For<ILogger<FindSymbolsTool>>(), _bus, null!)
        );
    }
}
