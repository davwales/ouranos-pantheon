using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.SearchSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.SearchSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.Tools.SearchSymbols;

public sealed class SearchSymbolsToolTests
{
    private static readonly SymbolAnalystContext Context = new(Factory.SymbolId, TimeFrame.OneDay);

    private static readonly Id<Symbol> OtherId = new("8c1f7f1e-0000-4000-8000-000000000003");

    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly SearchSymbolsTool _tool;

    public SearchSymbolsToolTests()
    {
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _tool = new SearchSymbolsTool(Substitute.For<ILogger<SearchSymbolsTool>>(), _bus);
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Context, arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenNamesMatch_ShouldReturnNamesWithIds()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(OtherSymbol()));

        // Act
        var result = await InvokeAsync("""{"name":" bandos "}""");

        // Assert
        result.ShouldContainData(
            new SymbolMatchesData(1, [new SymbolMatchData(OtherId, "Bandos godsword")])
        );

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetAllSymbolsInput(
                        Take: SearchSymbolsTool.MaxMatches,
                        Filter: [$"MarketId:eq:{Factory.MarketId}", "Name:like:bandos"]
                    )
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenMoreSymbolsMatchThanAreReturned_ShouldReportTheTotal()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(new PagedResponse<GetAllSymbolsResponse>([OtherSymbol()], 25, 0, 1));

        // Act
        var result = await InvokeAsync("""{"name":"bandos"}""");

        // Assert
        var data = result.ShouldContainData<SymbolMatchesData>();
        data.TotalMatches.ShouldBe(25);
        data.Symbols.Count.ShouldBe(1);
    }

    [Fact]
    public async Task InvokeAsync_WhenNothingMatches_ShouldReturnNoSymbols()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged<GetAllSymbolsResponse>());

        // Act
        var result = await InvokeAsync("""{"name":"Nothing"}""");

        // Assert
        result.ShouldContainData(new SymbolMatchesData(0, []));
    }

    [Fact]
    public async Task InvokeAsync_WhenNameIsBlank_ShouldSaySoWithoutQuerying()
    {
        // Act
        var result = await InvokeAsync("""{"name":"  "}""");

        // Assert
        result.ShouldContainNote().ShouldBe(SearchSymbolsTool.NoSearchText);

        await _bus.DidNotReceive()
            .InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SearchSymbolsTool(Substitute.For<ILogger<SearchSymbolsTool>>(), null!)
        );
    }

    private static GetAllSymbolsResponse OtherSymbol()
    {
        return new GetAllSymbolsResponse(
            OtherId,
            "11804",
            null,
            "Bandos godsword",
            Factory.MarketId
        );
    }
}
