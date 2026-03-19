using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;

public sealed record GetAllMarketsResponse(
    Id<Market> Id,
    string Name,
    Taxes Taxes,
    bool IsForecastingEnabled,
    string? Description,
    string? Icon
);
