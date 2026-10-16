using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.CompareSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.CompareSymbol;

public sealed class CompareSymbolTool(ILogger<CompareSymbolTool> logger, IMessageBus bus)
    : AssistantDataTool<SymbolAnalystContext, CompareSymbolArguments, ComparedSymbolData>(logger)
{
    internal const int StrongestSignals = 3;

    internal const string SameSymbol = "That is the symbol already shown above.";

    internal static readonly TimeFrame[] TimeFrames =
    [
        TimeFrame.OneDay,
        TimeFrame.OneWeek,
        TimeFrame.OneMonth,
    ];

    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "compare_symbol";

    public override string Description =>
        "Price summaries and strongest signals of another symbol in the same market, by the id "
        + "that search_symbols returns.";

    protected override async ValueTask<AssistantData<ComparedSymbolData>> QueryAsync(
        SymbolAnalystContext context,
        CompareSymbolArguments arguments,
        CancellationToken cancellationToken
    )
    {
        if (arguments.SymbolId == context.SymbolId)
        {
            return AssistantData<ComparedSymbolData>.WithNote(SameSymbol);
        }

        var current = await _bus.InvokeAsync<GetSymbolResponse>(
            new GetSymbolInput(context.SymbolId),
            cancellationToken
        );

        GetSymbolResponse other;
        try
        {
            other = await _bus.InvokeAsync<GetSymbolResponse>(
                new GetSymbolInput(arguments.SymbolId),
                cancellationToken
            );
        }
        catch (NotFoundException)
        {
            return NotInMarket(arguments.SymbolId);
        }

        if (other.MarketId != current.MarketId)
        {
            return NotInMarket(arguments.SymbolId);
        }

        var summaries = Task.WhenAll(
            TimeFrames.Select(timeFrame =>
                SymbolAnalystQueries.LoadPriceSummaryAsync(
                    _bus,
                    other.MarketId,
                    timeFrame,
                    other.Id,
                    cancellationToken
                )
            )
        );

        var signals = await _bus.InvokeAsync<GetSymbolSignalsResponse>(
            new GetSymbolSignalsInput(other.Id),
            cancellationToken
        );

        return new ComparedSymbolData(
            other.Name,
            await summaries,
            signals.Summary,
            [
                .. signals
                    .Signals.OrderByDescending(signal => Math.Abs(signal.Value))
                    .Take(StrongestSignals)
                    .Select(signal => SymbolAnalystData.Signal(signal)),
            ]
        );
    }

    internal static string NotInMarketNote(Id<Symbol> symbolId)
    {
        return $"No symbol with id '{symbolId}' in this market; use search_symbols to find one.";
    }

    private static AssistantData<ComparedSymbolData> NotInMarket(Id<Symbol> symbolId)
    {
        return AssistantData<ComparedSymbolData>.WithNote(NotInMarketNote(symbolId));
    }
}
