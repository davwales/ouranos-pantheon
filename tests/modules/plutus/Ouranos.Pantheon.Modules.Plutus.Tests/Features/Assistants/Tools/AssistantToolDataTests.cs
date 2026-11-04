using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools;

public sealed class AssistantToolDataTests
{
    [Fact]
    public void Signal_WhenGivenATrend_ShouldUseTheLabelAsTheName()
    {
        // Act
        var signal = AssistantToolData.Signal(Factory.Signals().Signals[0], [0.1m, 0.4m]);

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
    public void ForecastOutlook_ShouldTakeTheLatestPriceAndDaysOneAndSeven()
    {
        // Arrange
        var forecast = Factory.Forecast();

        // Act
        var outlook = AssistantToolData.ForecastOutlook(forecast);

        // Assert
        outlook.ShouldBe(
            new ForecastOutlookData(
                forecast.Latest.AveragePrice,
                forecast.DayOne.AveragePrice,
                forecast.DayOne.Gain,
                forecast.DaySeven.AveragePrice,
                forecast.DaySeven.Gain
            )
        );
    }

    [Fact]
    public void Percent_WhenGivenARatio_ShouldScaleItToPercent()
    {
        // Act
        var percent = AssistantToolData.Percent(0.0145m);

        // Assert
        percent.ShouldBe(1.45m);
    }

    [Fact]
    public void Truncate_WhenValueIsShort_ShouldOnlyJoinItsLines()
    {
        // Act
        var truncated = AssistantToolData.Truncate(" Line one\nline two ", 20);

        // Assert
        truncated.ShouldBe("Line one line two");
    }

    [Fact]
    public void Truncate_WhenValueIsLong_ShouldEndWithAnEllipsisWithinTheLimit()
    {
        // Act
        var truncated = AssistantToolData.Truncate(new string('x', 20), 10);

        // Assert
        truncated.ShouldBe($"{new string('x', 9)}…");
    }
}
