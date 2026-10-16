using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst;

public sealed class SymbolAnalystPromptTests
{
    [Fact]
    public void Compose_WhenAllDataIsPresent_ShouldRenderInstructionsAndOnlyTheHeadline()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot();

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldBe(
            SymbolAnalystPrompt.Instructions
                + """


                ## Symbol: Armadyl godsword
                - Code: 11802
                - Market: OSRS (Old School RuneScape Grand Exchange.)
                - Buy limit: 8
                - Alchemy values (fixed NPC sale prices, not market prices): high 750K, low 500K
                - Tax: 2% of the sell price per item (min 1, max 5M)
                - The user is viewing the OneDay chart
                - Data as of: 2026-10-08 12:00

                ## Today So Far
                Avg 10.1M, low 9.8M, high 10.4M, vol 40.
                """
        );
    }

    [Fact]
    public void Compose_WhenForecastingIsDisabled_ShouldSayForecastingIsDisabled()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with
        {
            Market = SymbolAnalystSnapshotFactory.Market(isForecastingEnabled: false),
        };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("- Forecasting is disabled for this market\n");
    }

    [Fact]
    public void Compose_WhenDataIsEmpty_ShouldRenderFallbackLines()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with
        {
            Market = SymbolAnalystSnapshotFactory.Market() with { Taxes = new Taxes(null) },
            Today = new GetDailySymbolSummaryResponse(0, 0, 0, 0),
        };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("- Tax: none");
        prompt.ShouldEndWith("## Today So Far\nNo trades yet today.");
    }

    [Fact]
    public void Compose_WhenSymbolHasSubcodeAndExchange_ShouldRenderThem()
    {
        // Arrange
        var symbol = new GetSymbolResponse(
            SymbolAnalystSnapshotFactory.SymbolId,
            "AAPL",
            "Common",
            "Apple Inc.",
            SymbolAnalystSnapshotFactory.MarketId,
            new AdditionalFields(Exchange: "NASDAQ")
        );
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with { Symbol = symbol };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("- Code: AAPL (Common)");
        prompt.ShouldContain("- Exchange: NASDAQ");
        prompt.ShouldNotContain("- Buy limit:");
        prompt.ShouldNotContain("Alchemy values");
    }

    [Fact]
    public void Compose_WhenOnlyHighAlchemyIsKnown_ShouldLabelItAsANonMarketPrice()
    {
        // Arrange
        var symbol = SymbolAnalystSnapshotFactory.Symbol() with
        {
            AdditionalFields = new AdditionalFields(HighAlch: 750000),
        };
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with { Symbol = symbol };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain(
            "- Alchemy values (fixed NPC sale prices, not market prices): high 750K\n"
        );
    }

    [Fact]
    public void Compose_WhenSnapshotIsFull_ShouldStayWithinTheContextBudgetWithEveryTool()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Full();

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        AssistantPromptBudget
            .Measure(prompt, SymbolAnalystToolFactory.All())
            .ShouldBeLessThanOrEqualTo(AssistantPromptBudget.Characters);
    }
}
