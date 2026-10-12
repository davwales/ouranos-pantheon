using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;

internal sealed record FitnessSegment(
    DateTimeOffset Start,
    DateTimeOffset End,
    BacktestScoreCache ScoreCache
);
