using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Swing;

internal static class SwingTestData
{
    public static readonly DateTimeOffset Start = new(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly FlatTax OsrsTax = new(1m, 5_000_000m, 0.02m);

    public static Market CreateMarket(FlatTax? tax = null)
    {
        return Market.Create(new Id<Market>(Guid.NewGuid().ToString()), "Market", new Taxes(tax));
    }

    public static Symbol CreateSymbol(Market market, string name, decimal? limit = null)
    {
        return Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            name,
            null,
            name,
            market.Id,
            new AdditionalFields(Limit: limit)
        );
    }

    public static DateOnly Day(DateTimeOffset date)
    {
        return DateOnly.FromDateTime(date.UtcDateTime);
    }

    /// <summary>One steady daily aggregate per day from <paramref name="from" /> to <paramref name="to" />.</summary>
    public static IEnumerable<DailyTradeAggregate> Steady(
        Symbol symbol,
        DateOnly from,
        DateOnly to,
        decimal price,
        decimal volume
    )
    {
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            yield return new DailyTradeAggregate(symbol.Id, day, price, price, price, volume);
        }
    }

    /// <summary>
    ///     Steady trading from <see cref="SwingMarketData.TypicalVolumeDays" /> days before
    ///     <see cref="Start" /> through <paramref name="days" /> days after it.
    /// </summary>
    public static IEnumerable<DailyTradeAggregate> Steady(
        Symbol symbol,
        int days,
        decimal price,
        decimal volume
    )
    {
        var start = Day(Start);
        return Steady(
            symbol,
            start.AddDays(-SwingMarketData.TypicalVolumeDays),
            start.AddDays(days),
            price,
            volume
        );
    }

    public static SwingSignalDay Signal(decimal value, bool isEligible = true, decimal price = 10m)
    {
        return new SwingSignalDay(
            price,
            isEligible,
            new Dictionary<SignalType, decimal> { [SignalType.TaxAdjustedRoi] = value }
        );
    }

    /// <summary>
    ///     Signals for every day from <see cref="Start" /> through <paramref name="days" /> days
    ///     after it; a null signal leaves the symbol out of that day.
    /// </summary>
    public static SwingSignalTable Signals(
        int days,
        IReadOnlyList<Symbol> symbols,
        Func<int, Symbol, SwingSignalDay?> signal
    )
    {
        var table = new Dictionary<DateOnly, IReadOnlyDictionary<Id<Symbol>, SwingSignalDay>>();
        for (var i = 0; i <= days; i++)
        {
            var day = new Dictionary<Id<Symbol>, SwingSignalDay>();
            foreach (var symbol in symbols)
            {
                if (signal(i, symbol) is { } value)
                {
                    day[symbol.Id] = value;
                }
            }

            table[Day(Start).AddDays(i)] = day;
        }

        return new SwingSignalTable(table);
    }
}
