using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Models.Forecasts;

public sealed record ForecastBucketIdDto(
    Id<Symbol> SymbolId,
    DateTime Bucket
);