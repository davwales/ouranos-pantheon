using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols;

public sealed class FindSymbolsTool(
    ILogger<FindSymbolsTool> logger,
    IMessageBus bus,
    IOptions<PlutusOptions> options
) : AssistantDataTool<IMarketScope, FindSymbolsArguments, ScreenedSymbolsData>(logger)
{
    private readonly IMessageBus _bus = Guard.Against.Null(bus);
    private readonly IOptions<PlutusOptions> _options = Guard.Against.Null(options);

    public override string Name => "find_symbols";

    public override string Description =>
        "Answer a question by screening every symbol in the market for the current period with "
        + "conditions (trade statistics with ROI in percent, or SymbolName with Like for part of "
        + "a name) and a sort. Also use it to find a symbol's id by name.";

    protected override async ValueTask<AssistantData<ScreenedSymbolsData>> QueryAsync(
        IMarketScope context,
        FindSymbolsArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var query = MarketTradesFilter.Build(arguments.Conditions, arguments.SortBy);

        var page = await _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
            new GetMarketTradesInput(
                context.MarketId,
                context.TimeFrame,
                query.SortField,
                query.SortField is null ? null
                    : arguments.Descending ? SortDirection.Desc.ToQueryValue()
                    : SortDirection.Asc.ToQueryValue(),
                Take: _options.Value.AssistantTools.ClampRows(arguments.Take),
                Filter: query.Filter
            ),
            cancellationToken
        );

        return new ScreenedSymbolsData(
            page.TotalCount,
            [.. page.Items.Select(Screened)],
            query.Problems.Count > 0 ? query.Problems : null
        );
    }

    private static ScreenedSymbolData Screened(GetMarketTradesResponse row)
    {
        return new ScreenedSymbolData(
            row.SymbolId,
            row.SymbolName,
            string.IsNullOrWhiteSpace(row.SymbolSubcode) ? null : row.SymbolSubcode,
            row.MinPrice,
            row.MaxPrice,
            row.TotalVolume > 0 ? row.AveragePrice : null,
            row.TotalVolume,
            row.NumTransactions,
            row.Margin,
            row.MinPrice > 0 ? row.Roi * 100 : null,
            row.TotalGain
        );
    }
}
