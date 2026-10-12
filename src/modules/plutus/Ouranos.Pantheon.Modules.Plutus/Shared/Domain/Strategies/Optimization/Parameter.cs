namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

/// <param name="Key">Stable name of the configuration field, e.g. <c>holdPeriodDays</c>.</param>
/// <param name="Min">Smallest value the optimizer may try.</param>
/// <param name="Max">Largest value the optimizer may try.</param>
/// <param name="IsWhole">Whether only whole numbers are valid.</param>
public sealed record Parameter(string Key, double Min, double Max, bool IsWhole = false)
{
    public double Clamp(double value)
    {
        var clamped = Math.Clamp(value, Min, Max);
        return IsWhole ? Math.Round(clamped) : clamped;
    }
}
