using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;

internal sealed record SymbolForecastInput(Symbol Symbol, List<ForecastModelPoint> HistoricalPoints);
