using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;

/// <param name="Positions">One closed position per round trip.</param>
/// <param name="Equity">The equity curve, sampled once per simulated day.</param>
/// <param name="FinalBalance">Cash after everything was liquidated.</param>
public sealed record SimulationResult(
    IReadOnlyList<BacktestPosition> Positions,
    EquityCurve Equity,
    decimal FinalBalance
)
{
    public BacktestResults ToResults(SimulationSettings settings)
    {
        return BacktestMetrics.Compute(
            settings.Budget,
            FinalBalance,
            Equity.MaxDrawdown,
            Equity.Peak,
            Positions,
            Equity.Values,
            settings.TotalDays
        );
    }
}
