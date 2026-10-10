using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;

internal sealed record SymbolAnalystSnapshot(
    GetSymbolResponse Symbol,
    GetMarketResponse Market,
    TimeFrame TimeFrame,
    GetSymbolTradesResponse SelectedTrades,
    IReadOnlyList<SymbolAnalystWindow> Windows,
    GetAllTradesResponse? LatestTrade,
    GetSymbolSignalsResponse Signals,
    GetSymbolSignalHistoryResponse SignalHistory,
    GetMarketForecastResponse? Forecast,
    IReadOnlyList<GetForecastEfficacyResponse> ForecastEfficacy,
    IReadOnlyList<GetAllPositionsResponse> Positions,
    DateTimeOffset GeneratedAt
);
