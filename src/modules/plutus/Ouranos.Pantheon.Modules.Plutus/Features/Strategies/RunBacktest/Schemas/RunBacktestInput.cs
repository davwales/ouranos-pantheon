using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record RunBacktestInput(
    Id<Strategy> StrategyId,
    Id<Market> MarketId,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m
);
