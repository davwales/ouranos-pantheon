using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetSignals;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.Tools.GetSignals;

public sealed class GetSignalsToolTests
{
    private static readonly SymbolAnalystContext Context = new(Factory.SymbolId, TimeFrame.OneDay);

    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetSignalsTool _tool;

    public GetSignalsToolTests()
    {
        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Signals());

        _bus.InvokeAsync<GetSymbolSignalHistoryResponse>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.SignalHistory());

        _tool = new GetSignalsTool(Substitute.For<ILogger<GetSignalsTool>>(), _bus);
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Context, arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenCalled_ShouldReturnTheScoreAndEverySignalWithItsWeeklyTrend()
    {
        // Act
        var result = await InvokeAsync("");

        // Assert
        result.ShouldContainData(
            new SignalsData(
                new SignalSummary(0.4m, 1, 0, 0, IsFlipFavourable: true, IsMerchFavourable: false),
                [
                    new SignalData(
                        "RSI",
                        0.4m,
                        SignalDirection.Bullish,
                        SignalStrength.Medium,
                        [InvestmentIntent.Buy, InvestmentIntent.Sell],
                        [-0.2m, 0.4m]
                    ),
                ]
            )
        );

        await _bus.Received(1)
            .InvokeAsync<GetSymbolSignalsResponse>(
                ArgExtensions.IsEquivalent(new GetSymbolSignalsInput(Factory.SymbolId)),
                Arg.Any<CancellationToken>()
            );

        await _bus.Received(1)
            .InvokeAsync<GetSymbolSignalHistoryResponse>(
                ArgExtensions.IsEquivalent(new GetSymbolSignalHistoryInput(Factory.SymbolId)),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenSignalHasNoHistory_ShouldOmitTheTrend()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolSignalHistoryResponse>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.SignalHistory() with { Signals = [] });

        // Act
        var result = await InvokeAsync("");

        // Assert
        result.Output.ShouldNotContain("weeklyTrend");
        result.ShouldContainData<SignalsData>().Signals[0].WeeklyTrend.ShouldBeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreNoSignals_ShouldReturnAnEmptyList()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Signals() with { Signals = [] });

        // Act
        var result = await InvokeAsync("");

        // Assert
        result.ShouldContainData<SignalsData>().Signals.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetSignalsTool(Substitute.For<ILogger<GetSignalsTool>>(), null!)
        );
    }
}
