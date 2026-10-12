namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

/// <summary>
///     How a backtest or optimization was requested, stored with it so a restart reruns exactly
///     the same job.
/// </summary>
/// <param name="VolumeParticipationRate">Share of traded volume the strategy can capture.</param>
/// <param name="SlippageMultiplier">Square-root market impact coefficient.</param>
/// <param name="Optimization">Set for optimization runs.</param>
public sealed record BacktestRunSettings(
    decimal VolumeParticipationRate = BacktestDataOptions.DefaultVolumeParticipationRate,
    decimal SlippageMultiplier = BacktestDataOptions.DefaultSlippageMultiplier,
    OptimizationSettings? Optimization = null
);
