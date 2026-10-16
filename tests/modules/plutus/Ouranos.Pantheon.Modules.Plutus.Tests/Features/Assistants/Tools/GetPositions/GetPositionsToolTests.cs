using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPositions;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools.GetPositions;

public sealed class GetPositionsToolTests
{
    private static readonly SymbolAnalystContext Context = new(
        Factory.MarketId,
        Factory.SymbolId,
        TimeFrame.OneDay
    );

    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetPositionsTool _tool;

    public GetPositionsToolTests()
    {
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(Factory.Positions().ToArray()));

        _tool = new GetPositionsTool(
            Substitute.For<ILogger<GetPositionsTool>>(),
            _bus,
            Options.Create(new PlutusOptions())
        );
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Context, arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusIsGiven_ShouldReturnOnlyPositionsWithThatStatusInTheSymbolsMarket()
    {
        // Act
        var result = await InvokeAsync("""{"status":"Bought"}""");

        // Assert
        result.ShouldContainData(
            new PositionsData([
                new PositionData(
                    Factory.GeneratedAt.AddDays(-2),
                    PositionSide.Buy,
                    PositionStatus.Bought,
                    2,
                    9900000,
                    "Bought the dip"
                ),
            ])
        );

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetAllPositionsInput(
                        Factory.MarketId,
                        Status: PositionStatus.Bought,
                        SortField: "CreatedAt",
                        SortDirection: "desc",
                        Take: new PlutusOptions().AssistantTools.MaxPositions,
                        Filter: [$"SymbolId:eq:{Factory.SymbolId}"]
                    )
                ),
                Arg.Any<CancellationToken>()
            );

        await _bus.DidNotReceive()
            .InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusIsNull_ShouldQueryEveryStatus()
    {
        // Act
        await InvokeAsync("""{"status":null}""");

        // Assert
        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetAllPositionsInput(
                        Factory.MarketId,
                        Status: null,
                        SortField: "CreatedAt",
                        SortDirection: "desc",
                        Take: new PlutusOptions().AssistantTools.MaxPositions,
                        Filter: [$"SymbolId:eq:{Factory.SymbolId}"]
                    )
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenNoteIsLong_ShouldTruncateItToOneLine()
    {
        // Arrange
        var position = Factory.Positions()[0] with
        {
            Notes = "Line one\n" + new string('x', 100),
        };

        _bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(position));

        // Act
        var result = await InvokeAsync("{}");

        // Assert
        result
            .ShouldContainData<PositionsData>()
            .Positions[0]
            .Notes.ShouldBe($"Line one {new string('x', GetPositionsTool.MaxNoteLength - 10)}…");
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreNoPositions_ShouldReturnAnEmptyList()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged<GetAllPositionsResponse>());

        // Act
        var result = await InvokeAsync("{}");

        // Assert
        result.ShouldContainData<PositionsData>().Positions.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetPositionsTool(
                Substitute.For<ILogger<GetPositionsTool>>(),
                null!,
                Options.Create(new PlutusOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetPositionsTool(Substitute.For<ILogger<GetPositionsTool>>(), _bus, null!)
        );
    }
}
