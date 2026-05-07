using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record BacktestParameters(
    Id<Market> MarketId,
    Strategy Strategy,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m,
    StrategyConfiguration? ConfigurationOverride = null
)
{
    public StrategyConfiguration Configuration => ConfigurationOverride ?? Strategy.Configuration;
    public int TotalDays => (int)(EndDate - StartDate).TotalDays;
}