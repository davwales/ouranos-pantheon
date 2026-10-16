using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools.GetPriceHistory;

public sealed class GetPriceHistoryToolTests
{
    private static readonly SymbolAnalystContext Context = new(
        Factory.MarketId,
        Factory.SymbolId,
        TimeFrame.OneDay
    );

    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetPriceHistoryTool _tool;

    public GetPriceHistoryToolTests()
    {
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _bus.InvokeAsync<GetSymbolTradesResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.SelectedTrades());

        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(Factory.OneDayTrades()));

        _bus.InvokeAsync<List<GetAllTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns([Factory.LatestTrade()]);

        _tool = new GetPriceHistoryTool(
            Substitute.For<ILogger<GetPriceHistoryTool>>(),
            _bus,
            Options.Create(new PlutusOptions())
        );
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Context, arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenCalled_ShouldReturnLatestTradeSummaryAndChartForTheTimeFrame()
    {
        // Act
        var result = await InvokeAsync("""{"timeFrame":"OneWeek"}""");

        // Assert
        result.ShouldContainData(
            new PriceHistoryData(
                new LatestTradeData(10300000, 2, Factory.GeneratedAt.AddMinutes(-5)),
                new PriceSummaryData(
                    TimeFrame.OneWeek,
                    HasTrades: true,
                    AveragePrice: 10100000,
                    LowPrice: 9800000,
                    HighPrice: 10400000,
                    Volume: 40,
                    Trades: 12,
                    MarginAfterTax: 392000,
                    RoiPercent: 4
                ),
                ChangePercent: 4.04m,
                [
                    new PriceBucketData(
                        Factory.GeneratedAt.AddHours(-2),
                        10000000,
                        9800000,
                        10200000,
                        25
                    ),
                    new PriceBucketData(
                        Factory.GeneratedAt.AddHours(-1),
                        10270000,
                        10100000,
                        10400000,
                        15
                    ),
                ]
            )
        );

        await _bus.Received(1)
            .InvokeAsync<GetSymbolTradesResponse>(
                ArgExtensions.IsEquivalent(
                    new GetSymbolTradesInput(
                        Factory.SymbolId,
                        TimeFrame.OneWeek,
                        new PlutusOptions().AssistantTools.ChartPoints
                    )
                ),
                Arg.Any<CancellationToken>()
            );

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetMarketTradesInput(
                        Factory.MarketId,
                        TimeFrame.OneWeek,
                        Take: 1,
                        Filter: [$"SymbolId:eq:{Factory.SymbolId}"]
                    )
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreNoTrades_ShouldReturnAnEmptyChartWithoutSummaryFigures()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolTradesResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new GetSymbolTradesResponse(0, 0, 0, 0, 0, 0, []));

        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged<GetMarketTradesResponse>());

        _bus.InvokeAsync<List<GetAllTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns([]);

        // Act
        var result = await InvokeAsync("""{"timeFrame":"OneWeek"}""");

        // Assert
        result.ShouldContainData(
            new PriceHistoryData(
                null,
                new PriceSummaryData(TimeFrame.OneWeek, HasTrades: false),
                null,
                []
            )
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenTimeFrameIsUnknown_ShouldReportInvalidArguments()
    {
        // Act
        var result = await InvokeAsync("""{"timeFrame":"Fortnight"}""");

        // Assert
        result.Output.ShouldBe(
            AssistantTool<SymbolAnalystContext, PriceHistoryArguments>.InvalidArgumentsOutput
        );

        await _bus.DidNotReceive()
            .InvokeAsync<GetSymbolTradesResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(TimeFrames))]
    public async Task InvokeAsync_ForEveryTimeFrame_ShouldCapTheChartAtTheConfiguredPoints(
        TimeFrame timeFrame
    )
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolTradesResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.FullTrades());

        // Act
        var result = await InvokeAsync($$"""{"timeFrame":"{{timeFrame}}"}""");

        // Assert
        result
            .ShouldContainData<PriceHistoryData>()
            .Chart.Count.ShouldBe(new PlutusOptions().AssistantTools.ChartPoints);
    }

    public static TheoryData<TimeFrame> TimeFrames()
    {
        return [.. Enum.GetValues<TimeFrame>()];
    }

    [Fact]
    public void Downsample_WhenThereAreMoreBucketsThanPoints_ShouldMergeTradedBucketsByVolume()
    {
        // Arrange
        var start = Factory.GeneratedAt;
        List<GetSymbolTradeBucketsResponse> buckets =
        [
            new(100, 1, 100, 90, 110, 1, start, 95, 105),
            new(200, 3, 600, 150, 250, 2, start.AddHours(1), 105, 210),
            new(300, 0, 0, 0, 0, 0, start.AddHours(2), 0, 0),
            new(400, 2, 800, 350, 450, 1, start.AddHours(3), 380, 420),
        ];

        // Act
        var points = GetPriceHistoryTool.Downsample(buckets, 2);

        // Assert
        points.ShouldBe([
            new GetSymbolTradeBucketsResponse(175, 4, 700, 90, 250, 3, start, 95, 210),
            new GetSymbolTradeBucketsResponse(
                400,
                2,
                800,
                350,
                450,
                1,
                start.AddHours(2),
                380,
                420
            ),
        ]);
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetPriceHistoryTool(
                Substitute.For<ILogger<GetPriceHistoryTool>>(),
                null!,
                Options.Create(new PlutusOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetPriceHistoryTool(Substitute.For<ILogger<GetPriceHistoryTool>>(), _bus, null!)
        );
    }
}
