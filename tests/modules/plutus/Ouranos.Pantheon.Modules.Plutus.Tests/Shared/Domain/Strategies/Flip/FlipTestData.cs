using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Flip;

internal static class FlipTestData
{
    public static readonly DateTimeOffset Start = new(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly FlatTax OsrsTax = new(1m, 5_000_000m, 0.02m);

    public static readonly MarketRules OsrsRules = new(OsrsTax);

    public static FlipSeries Series(
        int hours,
        Func<int, FlipBar> bar,
        decimal buyLimit = 1_000_000m,
        string name = "Item"
    )
    {
        var bars = new FlipBar[hours * FlipMarketData.BarsPerHour];
        for (var i = 0; i < bars.Length; i++)
        {
            bars[i] = bar(i);
        }

        return new FlipSeries(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            name,
            null,
            buyLimit,
            bars
        );
    }

    /// <summary>Insta-sells at <paramref name="low" /> and insta-buys at <paramref name="high" /> in every bucket.</summary>
    public static FlipSeries Bouncing(
        int hours,
        float low,
        float high,
        float volume = 1000f,
        decimal buyLimit = 1_000_000m,
        string name = "Item"
    )
    {
        return Series(hours, _ => new FlipBar(high, volume, low, volume), buyLimit, name);
    }

    public static FlipMarketData Market(params FlipSeries[] series)
    {
        return new FlipMarketData(Start, series[0].Bars.Length, series, OsrsRules);
    }
}
