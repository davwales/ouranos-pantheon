using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Scopes;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSymbol;

public sealed class GetSymbolTool(ILogger<GetSymbolTool> logger, IMessageBus bus)
    : AssistantDataTool<IMarketScope, GetSymbolArguments, MarketSymbolData>(logger)
{
    internal const int StrongestSignals = 3;

    internal const string SameSymbol = "That is the symbol already shown above.";

    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "get_symbol";

    public override string Description =>
        "Price summaries, strongest signals and forecast outlook for one symbol in this market, "
        + "by its id.";

    protected override async ValueTask<AssistantData<MarketSymbolData>> QueryAsync(
        IMarketScope context,
        GetSymbolArguments arguments,
        CancellationToken cancellationToken
    )
    {
        if (context is ISymbolScope anchored && anchored.SymbolId == arguments.SymbolId)
        {
            return AssistantData<MarketSymbolData>.WithNote(SameSymbol);
        }

        GetSymbolResponse symbol;
        try
        {
            symbol = await _bus.InvokeAsync<GetSymbolResponse>(
                new GetSymbolInput(arguments.SymbolId),
                cancellationToken
            );
        }
        catch (NotFoundException)
        {
            return NotInMarket(arguments.SymbolId);
        }

        if (symbol.MarketId != context.MarketId)
        {
            return NotInMarket(arguments.SymbolId);
        }

        var summaries = AssistantToolQueries.LoadPriceSummariesAsync(
            _bus,
            context.MarketId,
            symbol.Id,
            cancellationToken
        );

        var signals = _bus.InvokeAsync<GetSymbolSignalsResponse>(
            new GetSymbolSignalsInput(symbol.Id),
            cancellationToken
        );

        var forecast = LoadForecastAsync(context, symbol.Id, cancellationToken);
        var current = await signals;

        return new MarketSymbolData(
            symbol.Name,
            await summaries,
            current.Summary,
            [
                .. current
                    .Signals.OrderByDescending(signal => Math.Abs(signal.Value))
                    .Take(StrongestSignals)
                    .Select(signal => AssistantToolData.Signal(signal)),
            ],
            await forecast is { } loaded ? AssistantToolData.ForecastOutlook(loaded) : null
        );
    }

    internal static string NotInMarketNote(Id<Symbol> symbolId)
    {
        return $"No symbol with id '{symbolId}' in this market; search for it by name first.";
    }

    private static AssistantData<MarketSymbolData> NotInMarket(Id<Symbol> symbolId)
    {
        return AssistantData<MarketSymbolData>.WithNote(NotInMarketNote(symbolId));
    }

    private async Task<GetMarketForecastResponse?> LoadForecastAsync(
        IMarketScope context,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken
    )
    {
        if (
            !await AssistantToolQueries.IsForecastingEnabledAsync(
                _bus,
                context.MarketId,
                cancellationToken
            )
        )
        {
            return null;
        }

        return await AssistantToolQueries.LoadForecastAsync(
            _bus,
            context.MarketId,
            symbolId,
            cancellationToken
        );
    }
}
