using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;

/// <param name="VolumeParticipationRate">
///     Share of a symbol's volume an order may take; defaults to the rate of the strategy's latest
///     completed backtest, so recommendations are sized the way that backtest was.
/// </param>
public sealed record GetRecommendationsInput(
    Id<Strategy> StrategyId,
    Id<Market> MarketId,
    decimal Budget,
    decimal? VolumeParticipationRate = null
);
