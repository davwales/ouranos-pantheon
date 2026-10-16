using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.GetPositions;

public sealed class GetPositionsTool(
    ILogger<GetPositionsTool> logger,
    IMessageBus bus,
    IOptions<PlutusOptions> options
) : AssistantDataTool<SymbolAnalystContext, PositionsArguments, PositionsData>(logger)
{
    internal const int MaxNoteLength = 40;

    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    public override string Name => "get_positions";

    public override string Description =>
        "Call this before advising on the user's positions, holdings or trades in this symbol. "
        + "Returns their most recent positions, optionally only those with one status; Pending "
        + "means planned, not filled.";

    protected override async ValueTask<AssistantData<PositionsData>> QueryAsync(
        SymbolAnalystContext context,
        PositionsArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var symbol = await _bus.InvokeAsync<GetSymbolResponse>(
            new GetSymbolInput(context.SymbolId),
            cancellationToken
        );

        var positions = await _bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
            new GetAllPositionsInput(
                symbol.MarketId,
                Status: arguments.Status,
                SortField: nameof(GetAllPositionsResponse.CreatedAt),
                SortDirection: "desc",
                Take: _options.Value.SymbolAnalyst.MaxPositions,
                Filter: SymbolAnalystQueries.SymbolFilter(context.SymbolId)
            ),
            cancellationToken
        );

        return new PositionsData([
            .. positions.Items.Select(position => new PositionData(
                position.CreatedAt,
                position.Side,
                position.Status,
                position.Quantity,
                position.Cost,
                string.IsNullOrWhiteSpace(position.Notes)
                    ? null
                    : SymbolAnalystData.Truncate(position.Notes, MaxNoteLength)
            )),
        ]);
    }
}
