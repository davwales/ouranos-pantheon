namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

/// <param name="LookbackDays">Days of history loaded before the backtest start for signal warm-up.</param>
/// <param name="LimitWindowsPerDay">
///     How many per-symbol buy-limit windows (e.g. OSRS 4-hour GE limits) a trader realistically
///     uses per simulated day.
/// </param>
public sealed record BacktestDataOptions(int LookbackDays, decimal LimitWindowsPerDay)
{
    public const string SectionName = "BacktestData";

    public const decimal DefaultLimitWindowsPerDay = 2m;

    public BacktestDataOptions()
        : this(LookbackDays: 30, LimitWindowsPerDay: DefaultLimitWindowsPerDay) { }
}
