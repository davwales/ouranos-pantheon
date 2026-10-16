using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSymbol.Schemas;

public sealed record MarketSymbolData(
    string Name,
    IReadOnlyList<PriceSummaryData> PriceSummaries,
    SignalSummary SignalScore,
    IReadOnlyList<SignalData> StrongestSignals,
    ForecastOutlookData? Forecast
);
