using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.SetExplorerView.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.SetExplorerView;

public sealed class SetExplorerViewTool(ILogger<SetExplorerViewTool> logger, IMessageBus bus)
    : AssistantTool<IExplorerScope, ExplorerViewArguments>(logger)
{
    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "set_explorer_view";

    public override string Description =>
        "Propose a change to the table the user is viewing when they ask to show, filter or sort "
        + "it; call it once with the full view. It replaces the filter and optionally the sort. Conditions apply to the trade statistics (ROI in "
        + "percent) or to SymbolName (Like matches part of a name); an empty list clears the "
        + "filter.";

    public override bool IsSingleDeliveryPerReply => true;

    public override bool IsAvailable(IExplorerScope context)
    {
        return context.View is not null;
    }

    protected override async ValueTask<AssistantToolResult> ExecuteAsync(
        IExplorerScope context,
        ExplorerViewArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var query = MarketTradesFilter.Build(arguments.Conditions, arguments.SortBy);

        // A partly applied view would not be what the user asked for, so nothing is proposed
        // until every condition is valid.
        if (query.Problems.Count > 0)
        {
            return new AssistantToolResult(
                $"Nothing proposed.\n{string.Join("\n", query.Problems)}"
            );
        }

        var page = await _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
            new GetMarketTradesInput(
                context.MarketId,
                context.TimeFrame,
                Take: 1,
                Filter: query.Filter
            ),
            cancellationToken
        );

        return new AssistantToolResult(
            $"Proposed a view of {page.TotalCount} symbols "
                + $"({Describe(query, arguments.Descending)}); the user must apply it.",
            new SetExplorerViewAction(query.Conditions, query.SortField, arguments.Descending)
        );
    }

    private static string Describe(MarketTradesQuery query, bool descending)
    {
        var sort = query.SortField is null
            ? "totalGain desc"
            : $"{query.SortField} {(descending ? "desc" : "asc")}";

        return query.Filter.Length == 0
            ? $"sorted by {sort}"
            : $"sorted by {sort}; filter {string.Join(" and ", query.Filter)}";
    }
}
