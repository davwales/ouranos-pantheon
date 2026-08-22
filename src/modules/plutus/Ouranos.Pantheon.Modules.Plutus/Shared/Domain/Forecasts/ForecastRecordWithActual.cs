using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;

public sealed record ForecastRecordWithActual(
    Id<ForecastRecord> Id,
    Id<ForecastRun> RunId,
    Id<Market> MarketId,
    Id<Symbol> SymbolId,
    string ModelName,
    DateTimeOffset GeneratedAt,
    DateTimeOffset TargetAt,
    int HorizonDays,
    decimal PredictedAveragePrice,
    decimal? ActualAveragePrice,
    decimal? ActualMinPrice,
    decimal? ActualMaxPrice,
    decimal? ActualVolume
);
