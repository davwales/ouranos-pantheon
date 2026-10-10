using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst.MarketAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst;

public sealed class MarketAnalystPromptTests
{
    // Roughly 1,000 tokens: the inference model's limited context window must still fit the
    // conversation, the reasoning and the reply.
    private const int PromptCharacterBudget = 4000;

    [Fact]
    public void Compose_WhenAllDataIsPresent_ShouldRenderInstructionsAndEverySection()
    {
        // Arrange
        var snapshot = Factory.Snapshot();

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldBe(
            MarketAnalystPrompt.Instructions
                + """


                ## Market: OSRS
                Tax: 2% of sell price (min 1, max 5M).
                Data as of 2026-10-08 12:00.

                ## Pulse (OneDay)
                Avg price 1234.5, volume 2.5M, 48K trades.
                Trend (time avg/vol): 10-08 08:00 1150/400, 10-08 10:00 1200/0

                ## Activity (last 4 weeks, share of trades)
                Busiest: Sat 20:00 (45%), Thu 12:00 (29.5%), Mon 18:00 (25%)
                Quietest: Sun 03:00 (0.5%), Mon 18:00 (25%), Thu 12:00 (29.5%)

                ## Rows in view (11-12 of 42; sorted by Roi desc; filter TotalVolume:gte:2)
                Name | Min | Max | Avg | Vol | Txns | Margin | ROI | Gain
                Twisted bow | 1.4B | 1.5B | 1.5B | 2 | 2 | 85M | 6% | 170M
                Rune platebody | 38K | 39.5K | 38.5K | 12K | 1500 | 710 | 1.9% | 88.8K
                """
        );
    }

    [Fact]
    public void Compose_WhenMarketIsUntaxed_ShouldSayThereIsNoTax()
    {
        // Arrange
        var snapshot = Factory.Snapshot(market: Factory.Market(isTaxed: false));

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("Tax: none.");
    }

    [Fact]
    public void Compose_WhenViewHasNoSortOrFilter_ShouldDescribeTheDefaultGainOrder()
    {
        // Arrange
        var snapshot = Factory.Snapshot(context: new(Factory.MarketId, TimeFrame.OneWeek));

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("## Rows in view (1-2 of 42; sorted by Gain desc)\n");
    }

    [Fact]
    public void Compose_WhenViewIsEmpty_ShouldSayNothingMatches()
    {
        // Arrange
        var snapshot = Factory.Snapshot(view: Factory.View());

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldEndWith("## Rows in view\nNo symbols match the user's current view.");
    }

    [Fact]
    public void Compose_WhenThereIsNoActivity_ShouldOmitTheActivitySection()
    {
        // Arrange
        var snapshot = Factory.Snapshot(
            heatmap: new GetVolumeHeatmapResponse([new HeatmapCellResponse(0, 0, 0, 0)])
        );

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldNotContain("## Activity");
    }

    [Fact]
    public void Compose_WhenThereAreNoOverviewBuckets_ShouldSayThereWereNoTrades()
    {
        // Arrange
        var snapshot = Factory.Snapshot(overview: new GetMarketOverviewResponse(0, 0, 0, 0, []));

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("No trades in this period.");
    }

    [Fact]
    public void Compose_WhenRowHasSubcodeAndNoVolumeOrPrice_ShouldLabelSubcodeAndAvoidDivision()
    {
        // Arrange
        var row = new GetMarketTradesResponse(
            Factory.TwistedBowId,
            "Darksteel ore",
            "HQ",
            TotalSpent: 0,
            MinPrice: 0,
            MaxPrice: 0,
            TotalVolume: 0,
            NumTransactions: 0,
            Limit: 0,
            Tax: 0
        );
        var snapshot = Factory.Snapshot(view: Factory.View(row));

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("Darksteel ore (HQ) | 0 | 0 | - | 0 | 0 | 0 | - | 0");
    }

    [Fact]
    public void Compose_WhenViewIsFull_ShouldStayWithinTheContextBudget()
    {
        // Arrange
        var rows = Enumerable
            .Range(0, 12)
            .Select(i => Factory.TwistedBow() with { SymbolName = $"Ancestral robe top {i}" })
            .ToArray();
        var snapshot = Factory.Snapshot(view: Factory.View(rows));

        // Act
        var prompt = MarketAnalystPrompt.Compose(snapshot);

        // Assert
        (prompt.Length + AssistantFormatting.Instructions.Length).ShouldBeLessThanOrEqualTo(
            PromptCharacterBudget
        );
    }

    [Theory]
    [InlineData(950, "950")]
    [InlineData(9999.5, "9999.5")]
    [InlineData(12345, "12.3K")]
    [InlineData(-45000, "-45K")]
    [InlineData(2500000, "2.5M")]
    [InlineData(1200000000, "1.2B")]
    public void FormatCompact_ShouldAbbreviateLargeNumbers(decimal value, string expected)
    {
        // Act
        var formatted = MarketAnalystPrompt.FormatCompact(value);

        // Assert
        formatted.ShouldBe(expected);
    }
}
