using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.SetExplorerView;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.SetExplorerView.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskMarketAnalyst.MarketAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools.SetExplorerView;

public sealed class SetExplorerViewToolTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly SetExplorerViewTool _tool;

    public SetExplorerViewToolTests()
    {
        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.View(Factory.TwistedBow(), Factory.RunePlatebody()));

        _tool = new SetExplorerViewTool(Substitute.For<ILogger<SetExplorerViewTool>>(), _bus);
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Factory.Context(), arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenConditionsAreValid_ShouldReturnTheViewAsAnAction()
    {
        // Act
        var result = await InvokeAsync(
            """
            {"conditions":[{"field":"TotalVolume","operator":"Gte","value":"1k"},
             {"field":"SymbolName","operator":"Like","value":"rune"}],
             "sortBy":"Margin","descending":false}
            """
        );

        // Assert
        result.Output.ShouldBe(
            "Proposed a view of 42 symbols (sorted by margin asc; "
                + "filter totalVolume:gte:1000 and symbolName:like:rune); the user must apply it."
        );

        var action = result.Action.ShouldBeOfType<SetExplorerViewAction>();

        action.Conditions.ShouldBe([
            new ExplorerCondition("totalVolume", "gte", "1000"),
            new ExplorerCondition("symbolName", "like", "rune"),
        ]);

        action.SortField.ShouldBe("margin");
        action.Descending.ShouldBeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoSortOrFilterIsGiven_ShouldDescribeTheDefaultGainOrder()
    {
        // Act
        var result = await InvokeAsync("""{"conditions":[],"sortBy":null,"descending":true}""");

        // Assert
        result.Output.ShouldBe(
            "Proposed a view of 42 symbols (sorted by totalGain desc); the user must apply it."
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenAConditionIsInvalid_ShouldProposeNothing()
    {
        // Act
        var result = await InvokeAsync(
            """
            {"conditions":[{"field":"Roi","operator":"Like","value":"x"}],
             "sortBy":null,"descending":true}
            """
        );

        // Assert
        result.Action.ShouldBeNull();
        result.Output.ShouldBe(
            "Nothing proposed.\nIgnored Roi Like 'x': like works only on SymbolName."
        );

        await _bus.DidNotReceive()
            .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public void IsSingleActionPerReply_ShouldBeTrue()
    {
        // Act & Assert
        _tool.IsSingleActionPerReply.ShouldBeTrue();
    }

    [Fact]
    public void IsAvailable_ShouldOnlyBeTrueOnTheExplorer()
    {
        // Act & Assert
        _tool.IsAvailable(Factory.Context()).ShouldBeTrue();

        _tool
            .IsAvailable(new MarketAnalystContext(Factory.MarketId, TimeFrame.OneDay))
            .ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SetExplorerViewTool(Substitute.For<ILogger<SetExplorerViewTool>>(), null!)
        );
    }
}
