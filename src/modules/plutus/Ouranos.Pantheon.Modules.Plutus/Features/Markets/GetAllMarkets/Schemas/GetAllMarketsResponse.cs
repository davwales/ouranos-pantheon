using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;

public sealed record GetAllMarketsResponse(
    Id<Market> Id,
    string Name,
    Taxes Taxes,
    bool IsForecastingEnabled,
    string? Description,
    string? Icon
);
