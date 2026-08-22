using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed record BacktestParameters(
    Id<Market> MarketId,
    Strategy Strategy,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal Budget,
    decimal VolumeParticipationRate = 0.25m,
    decimal SlippageMultiplier = 0.1m,
    TradingConfiguration? ConfigurationOverride = null,
    List<InputWeight>? InputWeightsOverride = null,
    InputThresholds? ThresholdsOverride = null
)
{
    public TradingConfiguration Configuration =>
        ConfigurationOverride ?? Strategy.TradingConfiguration;

    public List<InputWeight> InputWeights => InputWeightsOverride ?? Strategy.InputWeights;

    public InputThresholds Thresholds => ThresholdsOverride ?? Strategy.Thresholds;

    public int TotalDays => (int)(EndDate - StartDate).TotalDays;
}
