using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSymbol.Schemas;

public sealed record MarketSymbolData(
    string Name,
    IReadOnlyList<PriceSummaryData> PriceSummaries,
    SignalSummary SignalScore,
    IReadOnlyList<SignalData> StrongestSignals,
    ForecastOutlookData? Forecast
);
