using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;

public sealed record GetForecastEfficacyResponse(
    Id<Symbol> SymbolId,
    string SymbolName,
    Id<Market> MarketId,
    string ModelName,
    int HorizonDays,
    int EvaluatedCount,
    decimal? MeanAbsoluteError,
    decimal? MeanAbsolutePercentageError,
    decimal? MeanBias,
    DateTimeOffset? FirstGeneratedAt,
    DateTimeOffset? LastGeneratedAt
);
