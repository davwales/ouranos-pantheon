

using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator.Schemas;

internal sealed record SymbolForecastInput(Symbol Symbol, List<ForecastPoint> HistoricalPoints);