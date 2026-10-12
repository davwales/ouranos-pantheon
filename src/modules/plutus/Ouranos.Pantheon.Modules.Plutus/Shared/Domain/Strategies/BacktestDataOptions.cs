namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

/// <param name="LookbackDays">Days of history loaded before the backtest start for signal warm-up.</param>
/// <param name="LimitWindowsPerDay">
///     How many per-symbol buy-limit windows a trader realistically uses per simulated day.
/// </param>
/// <param name="MinFillFraction">
///     Smallest share of a position's intended budget a liquidity- or limit-capped fill must reach
///     for the position to be taken at all.
/// </param>
public sealed record BacktestDataOptions(
    int LookbackDays,
    decimal LimitWindowsPerDay,
    decimal MinFillFraction
)
{
    public const string SectionName = "BacktestData";

    public const decimal DefaultLimitWindowsPerDay = 2m;

    public const decimal DefaultMinFillFraction = 0.1m;

    /// <summary>Share of a symbol's typical daily volume a trader can realistically fill.</summary>
    public const decimal DefaultVolumeParticipationRate = 0.1m;

    /// <summary>
    ///     Square-root impact coefficient; at the default participation it costs about half of
    ///     the ~3% median buy-side/sell-side spread measured on the liquid symbols it was calibrated on.
    /// </summary>
    public const decimal DefaultSlippageMultiplier = 0.05m;

    public BacktestDataOptions()
        : this(
            LookbackDays: 30,
            LimitWindowsPerDay: DefaultLimitWindowsPerDay,
            MinFillFraction: DefaultMinFillFraction
        ) { }
}
