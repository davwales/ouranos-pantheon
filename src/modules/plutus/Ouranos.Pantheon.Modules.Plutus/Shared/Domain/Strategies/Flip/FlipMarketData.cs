using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <param name="Start">Time of bar 0; always on a whole hour.</param>
/// <param name="BarCount">Bars per series.</param>
/// <param name="Series">One entry per symbol in the flip universe.</param>
/// <param name="Rules">How the market taxes sales.</param>
public sealed record FlipMarketData(
    DateTimeOffset Start,
    int BarCount,
    IReadOnlyList<FlipSeries> Series,
    MarketRules Rules
) : IStrategyMarketData
{
    public const int BarMinutes = 5;

    public const int BarsPerHour = 60 / BarMinutes;

    public const int BarsPerDay = BarsPerHour * 24;

    public int IndexOf(DateTimeOffset time)
    {
        return (int)Math.Floor((time - Start).TotalMinutes / BarMinutes);
    }

    public DateTimeOffset TimeOf(int index)
    {
        return Start.AddMinutes(index * BarMinutes);
    }
}
