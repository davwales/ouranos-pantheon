using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetActivityHours;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetActivityHours.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst.MarketAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst.Tools.GetActivityHours;

public sealed class GetActivityHoursToolTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetActivityHoursTool _tool;

    public GetActivityHoursToolTests()
    {
        _bus.InvokeAsync<GetVolumeHeatmapResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Heatmap());

        _tool = new GetActivityHoursTool(Substitute.For<ILogger<GetActivityHoursTool>>(), _bus);
    }

    private Task<AssistantToolResult> InvokeAsync()
    {
        return _tool.InvokeAsync(Factory.Context(), "{}", CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreTrades_ShouldReturnTheBusiestAndQuietestSlots()
    {
        // Act
        var result = await InvokeAsync();

        // Assert
        result.ShouldContainData(
            new ActivityHoursData(
                GetActivityHoursTool.LookbackWeeks,
                [
                    new ActivitySlotData(DayOfWeek.Saturday, 20, 45),
                    new ActivitySlotData(DayOfWeek.Thursday, 12, 29.5m),
                    new ActivitySlotData(DayOfWeek.Monday, 18, 25),
                ],
                [
                    new ActivitySlotData(DayOfWeek.Sunday, 3, 0.5m),
                    new ActivitySlotData(DayOfWeek.Monday, 18, 25),
                    new ActivitySlotData(DayOfWeek.Thursday, 12, 29.5m),
                ]
            )
        );

        await _bus.Received(1)
            .InvokeAsync<GetVolumeHeatmapResponse>(
                ArgExtensions.IsEquivalent(
                    new GetVolumeHeatmapInput(Factory.MarketId, GetActivityHoursTool.LookbackWeeks)
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenThereIsNoActivity_ShouldSaySo()
    {
        // Arrange
        _bus.InvokeAsync<GetVolumeHeatmapResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new GetVolumeHeatmapResponse([new HeatmapCellResponse(0, 0, 0, 0)]));

        // Act
        var result = await InvokeAsync();

        // Assert
        result.ShouldContainNote().ShouldBe(GetActivityHoursTool.NoTrades);
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetActivityHoursTool(Substitute.For<ILogger<GetActivityHoursTool>>(), null!)
        );
    }
}
