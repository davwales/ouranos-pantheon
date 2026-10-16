using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSymbol;

public sealed class GetSymbolTool(ILogger<GetSymbolTool> logger, IMessageBus bus)
    : AssistantDataTool<MarketAnalystContext, GetSymbolArguments, MarketSymbolData>(logger)
{
    internal const int StrongestSignals = 3;

    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public override string Name => "get_symbol";

    public override string Description =>
        "Price summaries, strongest signals and forecast outlook for one symbol.";

    protected override async ValueTask<AssistantData<MarketSymbolData>> QueryAsync(
        MarketAnalystContext context,
        GetSymbolArguments arguments,
        CancellationToken cancellationToken
    )
    {
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

        var summaries = SymbolAnalystQueries.LoadPriceSummariesAsync(
            _bus,
            symbol.MarketId,
            symbol.Id,
            cancellationToken
        );

        var signals = _bus.InvokeAsync<GetSymbolSignalsResponse>(
            new GetSymbolSignalsInput(symbol.Id),
            cancellationToken
        );

        var forecast = LoadForecastAsync(symbol, cancellationToken);
        var current = await signals;

        return new MarketSymbolData(
            symbol.Name,
            await summaries,
            current.Summary,
            [
                .. current
                    .Signals.OrderByDescending(signal => Math.Abs(signal.Value))
                    .Take(StrongestSignals)
                    .Select(signal => SymbolAnalystData.Signal(signal)),
            ],
            await forecast is { } loaded ? MarketAnalystData.ForecastOutlook(loaded) : null
        );
    }

    internal static string NotInMarketNote(Id<Symbol> symbolId)
    {
        return $"No symbol with id '{symbolId}' in this market; use find_symbols to find one.";
    }

    private static AssistantData<MarketSymbolData> NotInMarket(Id<Symbol> symbolId)
    {
        return AssistantData<MarketSymbolData>.WithNote(NotInMarketNote(symbolId));
    }

    private async Task<GetMarketForecastResponse?> LoadForecastAsync(
        GetSymbolResponse symbol,
        CancellationToken cancellationToken
    )
    {
        var market = await _bus.InvokeAsync<GetMarketResponse>(
            new GetMarketInput(symbol.MarketId),
            cancellationToken
        );

        if (!market.IsForecastingEnabled)
        {
            return null;
        }

        var forecasts = await _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
            new GetMarketForecastInput(
                market.Id,
                Take: 1,
                Filter: SymbolAnalystQueries.SymbolFilter(symbol.Id)
            ),
            cancellationToken
        );

        return forecasts.Items.FirstOrDefault();
    }
}
