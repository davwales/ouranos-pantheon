using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetForecastLeaders.Schemas;

public sealed record ForecastLeaderData(
    Id<Symbol> SymbolId,
    string Name,
    ForecastOutlookData Forecast
);
