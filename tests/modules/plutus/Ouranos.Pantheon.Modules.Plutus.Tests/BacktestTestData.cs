using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests;

internal static class BacktestTestData
{
    /// <summary>
    ///     Daily aggregates for the <see cref="SwingMarketData.TypicalVolumeDays" /> days before
    ///     <paramref name="date" /> at a steady price and volume, so the liquidity cap on
    ///     <paramref name="date" /> is driven by that day's own aggregate.
    /// </summary>
    public static List<DailyTradeAggregate> History(
        Id<Symbol> symbolId,
        DateTimeOffset date,
        decimal price,
        decimal volume
    )
    {
        var day = DateOnly.FromDateTime(date.UtcDateTime);
        return
        [
            .. Enumerable
                .Range(1, SwingMarketData.TypicalVolumeDays)
                .Select(i => new DailyTradeAggregate(
                    symbolId,
                    day.AddDays(-i),
                    price,
                    price,
                    price,
                    volume
                ))
                .Reverse(),
        ];
    }
}
