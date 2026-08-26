namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

/// <summary>
///     Tunable knobs for the genetic-algorithm optimizer. Fitness-component weights
///     (Sortino/CAGR/drawdown/turnover/L1) are intentionally not on this record - they
///     are per-request and ride on <c>OptimizeStrategyMessage</c> from the API body,
///     so they can be tuned per optimization run without restarting the host. The
///     out-of-sample split ratio likewise rides on <c>OptimizeStrategyMessage</c>.
/// </summary>
public sealed record OptimizationOptions(double ElitismRate, double MutationRate)
{
    public const string SectionName = "Optimization";

    public OptimizationOptions()
        : this(ElitismRate: 0.3, MutationRate: 0.1) { }
}
