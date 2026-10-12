using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;

public sealed record BacktestOutcome(
    BacktestResults Results,
    IReadOnlyList<BacktestPosition> Positions
);
