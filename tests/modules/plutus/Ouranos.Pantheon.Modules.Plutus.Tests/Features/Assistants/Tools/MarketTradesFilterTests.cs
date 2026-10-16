using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools;

public sealed class MarketTradesFilterTests
{
    [Fact]
    public void Build_WhenConditionsAreValid_ShouldNormalizeThemIntoFilterStrings()
    {
        // Arrange
        MarketTradesCondition[] conditions =
        [
            new(MarketTradesField.Roi, MarketTradesOperator.Gt, "10%"),
            new(MarketTradesField.TotalVolume, MarketTradesOperator.Gte, "1.5m"),
            new(MarketTradesField.SymbolName, MarketTradesOperator.Like, " rune "),
            new(MarketTradesField.Margin, MarketTradesOperator.Lt, "2,500"),
        ];

        // Act
        var query = MarketTradesFilter.Build(conditions, MarketTradesField.TotalGain);

        // Assert
        query.Problems.ShouldBeEmpty();

        query.Filter.ShouldBe([
            "roi:gt:0.1",
            "totalVolume:gte:1500000",
            "symbolName:like:rune",
            "margin:lt:2500",
        ]);

        query.Conditions[0].ShouldBe(new ExplorerCondition("roi", "gt", "0.1"));
        query.SortField.ShouldBe("totalGain");
    }

    [Theory]
    [InlineData(
        MarketTradesField.Roi,
        MarketTradesOperator.Like,
        "5",
        "like works only on SymbolName"
    )]
    [InlineData(MarketTradesField.Margin, MarketTradesOperator.Gt, "lots", "not a number")]
    [InlineData(MarketTradesField.Margin, MarketTradesOperator.Gt, " ", "no value")]
    [InlineData(
        MarketTradesField.SymbolName,
        MarketTradesOperator.Gt,
        "a",
        "names support only eq, neq and like"
    )]
    [InlineData(
        MarketTradesField.SymbolName,
        MarketTradesOperator.Eq,
        "a|b",
        "it contains a reserved character"
    )]
    public void Build_WhenConditionIsInvalid_ShouldDropItAndExplainWhy(
        MarketTradesField field,
        MarketTradesOperator op,
        string value,
        string reason
    )
    {
        // Act
        var query = MarketTradesFilter.Build([new(field, op, value)], null);

        // Assert
        query.Filter.ShouldBeEmpty();
        query.Problems.ShouldBe([$"Ignored {field} {op} '{value}': {reason}."]);
    }

    [Fact]
    public void Build_WhenSortFieldIsNotSortable_ShouldReportIt()
    {
        // Act
        var query = MarketTradesFilter.Build([], MarketTradesField.SymbolName);

        // Assert
        query.SortField.ShouldBeNull();
        query.Problems.ShouldBe(["Cannot sort by SymbolName."]);
    }
}
