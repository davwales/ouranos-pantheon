using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetForecast;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.Tools.GetForecast;

public sealed class GetForecastToolTests
{
    private static readonly SymbolAnalystContext Context = new(Factory.SymbolId, TimeFrame.OneDay);

    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetForecastTool _tool;

    public GetForecastToolTests()
    {
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market());

        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(Factory.Forecast()));

        _bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(Factory.ForecastEfficacy().ToArray()));

        _tool = new GetForecastTool(Substitute.For<ILogger<GetForecastTool>>(), _bus);
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Context, arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenForecastingIsEnabled_ShouldReturnEveryDayWithTheError()
    {
        // Act
        var result = await InvokeAsync("{}");

        // Assert
        var data = result.ShouldContainData<ForecastData>();
        data.Baseline.ShouldBeEquivalentTo(new ForecastPriceData(10100000, 9800000, 10400000, 40));
        data.Days.Select(day => day.Day).ShouldBe([1, 2, 3, 4, 5, 6, 7]);

        data.Days[0]
            .ShouldBeEquivalentTo(new ForecastDayData(1, 10200000, 10000000, 10500000, 290000));

        data.Days[6].AveragePrice.ShouldBe(10600000);
        data.Error.ShouldNotBeNull();
        data.Error.ModelName.ShouldBe("plutus-forecasting-v1");
        data.Error.WindowDays.ShouldBe(30);
        data.Error.EvaluatedCount.ShouldBe(20);
        data.Error.Horizons.ShouldBe([new ForecastHorizonErrorData(1, 1.45m)]);
        data.Error.MeanForecastMinusActual.ShouldBe(-20000);
    }

    [Fact]
    public async Task InvokeAsync_WhenForecastingIsDisabled_ShouldSaySoWithoutQueryingForecasts()
    {
        // Arrange
        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market(isForecastingEnabled: false));

        // Act
        var result = await InvokeAsync("{}");

        // Assert
        result.ShouldContainNote().ShouldBe(GetForecastTool.ForecastingDisabled);

        await _bus.DidNotReceive()
            .InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenThereIsNoForecast_ShouldSaySo()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged<GetMarketForecastResponse>());

        // Act
        var result = await InvokeAsync("{}");

        // Assert
        result.ShouldContainNote().ShouldBe(GetForecastTool.NoForecast);
    }

    [Fact]
    public void Error_WhenEfficacySpansSeveralModels_ShouldUseTheMostEvaluatedModel()
    {
        // Arrange
        var row = Factory.ForecastEfficacy()[0];
        IReadOnlyList<GetForecastEfficacyResponse> efficacy =
        [
            row with
            {
                ModelName = "old",
                EvaluatedCount = 2,
                MeanAbsolutePercentageError = 0.5m,
            },
            row with
            {
                HorizonDays = 7,
                MeanAbsolutePercentageError = 0.04m,
                MeanBias = 10,
            },
            row with
            {
                HorizonDays = 3,
                MeanAbsolutePercentageError = null,
            },
            row,
        ];

        // Act
        var error = GetForecastTool.Error(efficacy);

        // Assert
        error.ShouldNotBeNull();
        error.ModelName.ShouldBe("plutus-forecasting-v1");

        error.Horizons.ShouldBe([
            new ForecastHorizonErrorData(1, 1.45m),
            new ForecastHorizonErrorData(7, 4m),
        ]);

        error.MeanForecastMinusActual.ShouldBe(-20000);
    }

    [Fact]
    public void Error_WhenThereIsNoEfficacy_ShouldBeNull()
    {
        // Act
        var error = GetForecastTool.Error([]);

        // Assert
        error.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetForecastTool(Substitute.For<ILogger<GetForecastTool>>(), null!)
        );
    }
}
