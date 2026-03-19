using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;

public sealed record GetMarketResponse(
    Id<Market> Id,
    string Name,
    Taxes Taxes,
    bool IsForecastingEnabled,
    string? Description,
    string? Icon
);
