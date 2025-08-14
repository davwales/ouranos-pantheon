using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Models.Forecasts;

public sealed record ForecastBucketIdDto(
    Id<Symbol> SymbolId,
    DateTime Bucket
);