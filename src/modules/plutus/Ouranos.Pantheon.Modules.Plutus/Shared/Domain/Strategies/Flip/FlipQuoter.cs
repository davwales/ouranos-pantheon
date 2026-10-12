using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <summary>
///     Prices buy and sell orders from recent side-aware trades and ranks symbols by the profit
///     those orders would have made per check-in. Shared by the flip simulator and flip
///     recommendations, so both pick symbols the same way.
/// </summary>
public static class FlipQuoter
{
    /// <summary>Share of 5-minute buckets in the lookback that must have traded on both sides.</summary>
    public const double MinPairedShare = 0.6;

    public const int PriceMoveDays = 7;

    /// <summary>Per-symbol buy limits apply over a rolling window of this many hours.</summary>
    public const int BuyLimitHours = 4;

    /// <summary>Quotes from the <see cref="FlipConfiguration.LookbackHours" /> before <paramref name="endHour" />.</summary>
    public static FlipQuote? Quote(
        FlipSeries series,
        int endHour,
        FlipConfiguration configuration,
        MarketRules rules
    )
    {
        var lookback = configuration.LookbackHours;
        if (lookback <= 0 || endHour - lookback < 0 || endHour > series.Hours)
        {
            return null;
        }

        var window = series.Window(endHour - lookback, endHour);
        if (
            window.HighVolume <= 0
            || window.LowVolume <= 0
            || window.PairedBars < MinPairedShare * lookback * FlipMarketData.BarsPerHour
        )
        {
            return null;
        }

        var spread = window.HighPrice - window.LowPrice;
        var bid = Math.Floor((decimal)(window.LowPrice + (double)configuration.BidOffset * spread));
        var ask = Math.Ceiling(
            (decimal)(window.HighPrice - (double)configuration.AskOffset * spread)
        );

        if (bid <= 0 || bid < configuration.MinPrice)
        {
            return null;
        }

        if (configuration.MaxPriceMove > 0)
        {
            var week = series.Window(endHour - (PriceMoveDays * 24), endHour);
            if (
                week.LowVolume <= 0
                || Math.Abs((window.LowPrice / week.LowPrice) - 1)
                    > (double)configuration.MaxPriceMove
            )
            {
                return null;
            }
        }

        return new FlipQuote(series, bid, ask, (decimal)window.LowPrice, rules.NetMargin(bid, ask));
    }

    public static decimal ExpectedUnits(
        FlipQuote quote,
        int endHour,
        FlipConfiguration configuration,
        decimal participationRate
    )
    {
        var bars = quote.Series.Bars;
        var bid = (double)quote.Bid;
        var ask = (double)quote.Ask;

        double buyable = 0;
        double sellable = 0;

        var to = Math.Min(endHour * FlipMarketData.BarsPerHour, bars.Length);
        for (
            var i = Math.Max(
                0,
                (endHour - configuration.LookbackHours) * FlipMarketData.BarsPerHour
            );
            i < to;
            i++
        )
        {
            var bar = bars[i];
            if (bar.LowVolume > 0 && bar.LowPrice <= bid)
            {
                buyable += bar.LowVolume;
            }

            if (bar.HighVolume > 0 && bar.HighPrice >= ask)
            {
                sellable += bar.HighVolume;
            }
        }

        var perCheckIn =
            (decimal)Math.Min(buyable, sellable)
            * participationRate
            * configuration.CheckInHours
            / configuration.LookbackHours;

        return Math.Min(
            perCheckIn,
            quote.Series.BuyLimit * configuration.CheckInHours / BuyLimitHours
        );
    }

    /// <summary>Symbols worth an order at <paramref name="endHour" />, most profitable first.</summary>
    public static List<FlipCandidate> Rank(
        IEnumerable<FlipSeries> series,
        int endHour,
        FlipConfiguration configuration,
        MarketRules rules,
        decimal participationRate
    )
    {
        var candidates = new List<FlipCandidate>();
        foreach (var s in series)
        {
            var quote = Quote(s, endHour, configuration, rules);
            if (quote is null || quote.NetMargin < configuration.MinMargin)
            {
                continue;
            }

            var units = ExpectedUnits(quote, endHour, configuration, participationRate);
            if (units < 1)
            {
                continue;
            }

            candidates.Add(new FlipCandidate(quote, units, quote.NetMargin * quote.Bid * units));
        }

        return [.. candidates.OrderByDescending(c => c.ExpectedProfit)];
    }
}
