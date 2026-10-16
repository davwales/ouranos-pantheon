using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.SearchSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.SearchSymbols;

public sealed class SearchSymbolsTool(ILogger<SearchSymbolsTool> logger, IMessageBus bus)
    : AssistantDataTool<SymbolAnalystContext, SearchSymbolsArguments, SymbolMatchesData>(logger)
{
    internal const int MaxMatches = 10;

    internal const string NoSearchText = "No search text was given.";

    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "search_symbols";

    public override string Description =>
        "Find symbols in this market whose name contains the given text, with their ids for "
        + "compare_symbol.";

    protected override async ValueTask<AssistantData<SymbolMatchesData>> QueryAsync(
        SymbolAnalystContext context,
        SearchSymbolsArguments arguments,
        CancellationToken cancellationToken
    )
    {
        var name = arguments.Name.Trim();
        if (name.Length == 0)
        {
            return AssistantData<SymbolMatchesData>.WithNote(NoSearchText);
        }

        var current = await _bus.InvokeAsync<GetSymbolResponse>(
            new GetSymbolInput(context.SymbolId),
            cancellationToken
        );

        var page = await _bus.InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(
            new GetAllSymbolsInput(
                Take: MaxMatches,
                Filter: [$"MarketId:eq:{current.MarketId}", $"Name:like:{name}"]
            ),
            cancellationToken
        );

        return new SymbolMatchesData(
            page.TotalCount,
            [.. page.Items.Select(symbol => new SymbolMatchData(symbol.Id, symbol.Name))]
        );
    }
}
