using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecastLeaders;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecastLeaders.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskMarketAnalyst.MarketAnalystSnapshotFactory;
using SymbolFactory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools.GetForecastLeaders;

public sealed class GetForecastLeadersToolTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetForecastLeadersTool _tool;

    public GetForecastLeadersToolTests()
    {
        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market());

        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(SymbolFactory.Paged(SymbolFactory.Forecast()));

        _tool = new GetForecastLeadersTool(
            Substitute.For<ILogger<GetForecastLeadersTool>>(),
            _bus,
            Options.Create(new PlutusOptions())
        );
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Factory.Context(), arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenForecastsExist_ShouldRankByDayOneGain()
    {
        // Act
        var result = await InvokeAsync("""{"take":5}""");

        // Assert
        result.ShouldContainData(
            new ForecastLeadersData([
                new ForecastLeaderData(
                    SymbolFactory.SymbolId,
                    "Armadyl godsword",
                    new ForecastOutlookData(10100000, 10200000, 2320000, 10600000, 2320000)
                ),
            ])
        );

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Is<GetMarketForecastInput>(i =>
                    i.SortField == "DayOne.Gain" && i.SortDirection == "desc" && i.Take == 5
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenForecastingIsDisabled_ShouldSaySoWithoutQueryingForecasts()
    {
        // Arrange
        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market() with { IsForecastingEnabled = false });

        // Act
        var result = await InvokeAsync("""{"take":null}""");

        // Assert
        result.ShouldContainNote().ShouldBe(AssistantToolQueries.ForecastingDisabled);

        await _bus.DidNotReceive()
            .InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreNoForecasts_ShouldSaySo()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(SymbolFactory.Paged<GetMarketForecastResponse>());

        // Act
        var result = await InvokeAsync("""{"take":null}""");

        // Assert
        result.ShouldContainNote().ShouldBe(GetForecastLeadersTool.NoForecasts);
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetForecastLeadersTool(
                Substitute.For<ILogger<GetForecastLeadersTool>>(),
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
            new GetForecastLeadersTool(
                Substitute.For<ILogger<GetForecastLeadersTool>>(),
                _bus,
                null!
            )
        );
    }
}
