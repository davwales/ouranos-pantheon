using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst;

public sealed class SymbolAnalystDataTests
{
    [Fact]
    public void Signal_WhenGivenATrend_ShouldUseTheLabelAsTheName()
    {
        // Act
        var signal = SymbolAnalystData.Signal(Factory.Signals().Signals[0], [0.1m, 0.4m]);

        // Assert
        signal.ShouldBeEquivalentTo(
            new SignalData(
                "RSI",
                0.4m,
                SignalDirection.Bullish,
                SignalStrength.Medium,
                [InvestmentIntent.Buy, InvestmentIntent.Sell],
                [0.1m, 0.4m]
            )
        );
    }

    [Fact]
    public void Percent_WhenGivenARatio_ShouldScaleItToPercent()
    {
        // Act
        var percent = SymbolAnalystData.Percent(0.0145m);

        // Assert
        percent.ShouldBe(1.45m);
    }

    [Fact]
    public void Truncate_WhenValueIsShort_ShouldOnlyJoinItsLines()
    {
        // Act
        var truncated = SymbolAnalystData.Truncate(" Line one\nline two ", 20);

        // Assert
        truncated.ShouldBe("Line one line two");
    }

    [Fact]
    public void Truncate_WhenValueIsLong_ShouldEndWithAnEllipsisWithinTheLimit()
    {
        // Act
        var truncated = SymbolAnalystData.Truncate(new string('x', 20), 10);

        // Assert
        truncated.ShouldBe($"{new string('x', 9)}…");
    }
}
