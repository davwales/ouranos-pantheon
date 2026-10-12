using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <summary>
///     Simulates a trader who checks in every <see cref="FlipConfiguration.CheckInHours" />.
///     At each check-in, unfilled buy orders are cancelled, everything bought since the last
///     check-in is listed, and free order slots get new buy orders for the best-ranked symbols.
///     Every open order counts toward <see cref="FlipConfiguration.MaxOpenOrders" />; inventory
///     beyond the limit waits unlisted.
///     <para>
///         Fills are evaluated per 5-minute bucket and never look ahead: a buy order fills when
///         that bucket's sell-side price is at or below the bid (a seller accepted that price,
///         so a resting bid at least as high was hit first), a sell order when the buy-side
///         price is at or above the ask. Each fill is capped by the participation rate x that
///         side's volume, and buys by the buy limit per rolling four hours.
///     </para>
/// </summary>
public sealed class FlipSimulator
{
    private readonly FlipMarketData _data;
    private readonly SimulationSettings _settings;
    private readonly FlipConfiguration _configuration;
    private readonly Dictionary<int, FlipHolding> _holdings = [];
    private readonly Dictionary<int, FlipBuyOrder> _buyOrders = [];
    private readonly Dictionary<int, Queue<(int Bar, decimal Quantity)>> _recentBuys = [];
    private readonly List<BacktestPosition> _positions = [];
    private readonly Dictionary<FlipSeries, int> _indexBySeries = [];
    private readonly EquityCurve _equity;
    private decimal _cash;

    private FlipSimulator(
        FlipMarketData data,
        FlipConfiguration configuration,
        SimulationSettings settings
    )
    {
        _data = data;
        _settings = settings;
        _configuration = configuration;
        _cash = settings.Budget;
        _equity = new EquityCurve(settings.Budget);
        for (var i = 0; i < data.Series.Count; i++)
        {
            _indexBySeries[data.Series[i]] = i;
        }
    }

    public static SimulationResult Run(
        FlipMarketData data,
        FlipConfiguration configuration,
        SimulationSettings settings
    )
    {
        Guard.Against.Null(data);
        Guard.Against.Null(configuration);
        Guard.Against.Null(settings);
        Guard.Against.NegativeOrZero(configuration.CheckInHours);
        Guard.Against.NegativeOrZero(configuration.MaxOpenOrders);

        return new FlipSimulator(data, configuration, settings).Simulate();
    }

    /// <summary>
    ///     Buy orders for the best candidates that fit the free order slots, the per-symbol budget and the
    ///     cash left. Shared by the simulator and live recommendations so both size orders the same
    ///     way.
    /// </summary>
    public static IEnumerable<(FlipCandidate Candidate, decimal Quantity)> PlanOrders(
        IEnumerable<FlipCandidate> candidates,
        int freeOrders,
        decimal positionBudget,
        decimal cash,
        Func<FlipSeries, decimal> heldCost
    )
    {
        foreach (var candidate in candidates)
        {
            if (freeOrders <= 0)
            {
                yield break;
            }

            var bid = candidate.Quote.Bid;
            var quantity = Math.Floor(
                Math.Min(
                    candidate.ExpectedUnits,
                    Math.Min(
                        Math.Max(0m, positionBudget - heldCost(candidate.Quote.Series)) / bid,
                        cash / bid
                    )
                )
            );
            if (quantity <= 0)
            {
                continue;
            }

            cash -= quantity * bid;
            freeOrders--;
            yield return (candidate, quantity);
        }
    }

    private SimulationResult Simulate()
    {
        var first = Math.Max(0, _data.IndexOf(_settings.Start));
        first -= first % FlipMarketData.BarsPerHour;
        var end = Math.Min(_data.BarCount, _data.IndexOf(_settings.End));
        var checkInBars = _configuration.CheckInHours * FlipMarketData.BarsPerHour;

        for (var bar = first; bar < end; bar++)
        {
            if ((bar - first) % FlipMarketData.BarsPerDay == 0)
            {
                _settings.CancellationToken.ThrowIfCancellationRequested();
                _settings.Progress?.Report((double)(bar - first) / Math.Max(1, end - first));
                _equity.Record(Equity());
            }

            if ((bar - first) % checkInBars == 0)
            {
                CheckIn(bar);
            }

            FillSells(bar);
            FillBuys(bar);
        }

        CancelBuyOrders();
        foreach (var (index, holding) in _holdings.ToList())
        {
            Sell(index, holding, holding.Quantity, holding.LastLowPrice, Math.Max(first, end - 1));
        }

        _equity.Record(_cash);
        _settings.Progress?.Report(1);
        return new SimulationResult(_positions, _equity, _cash);
    }

    private void CheckIn(int bar)
    {
        CancelBuyOrders();
        var hour = bar / FlipMarketData.BarsPerHour;
        var equity = Equity();

        var freeOrders = _configuration.MaxOpenOrders - ListInventory(hour, bar);
        if (freeOrders <= 0)
        {
            return;
        }

        var candidates = FlipQuoter.Rank(
            _data.Series,
            hour,
            _configuration,
            _data.Rules,
            _settings.ParticipationRate
        );
        var orders = PlanOrders(
            candidates,
            freeOrders,
            equity * _configuration.MaxPositionPercent,
            _cash,
            series =>
                _holdings.TryGetValue(_indexBySeries[series], out var holding) ? holding.Cost : 0m
        );

        foreach (var (candidate, quantity) in orders)
        {
            _cash -= quantity * candidate.Quote.Bid;
            _buyOrders[_indexBySeries[candidate.Quote.Series]] = new FlipBuyOrder(
                candidate.Quote.Bid,
                quantity
            );
        }
    }

    private int ListInventory(int hour, int bar)
    {
        var sellOrders = 0;
        var maxHoldBars = _configuration.MaxHoldHours * FlipMarketData.BarsPerHour;
        foreach (var (index, holding) in _holdings.OrderByDescending(h => h.Value.Cost))
        {
            if (sellOrders >= _configuration.MaxOpenOrders)
            {
                holding.Listed = 0;
                continue;
            }

            sellOrders++;
            holding.Listed = holding.Quantity;
            var quote = FlipQuoter.Quote(_data.Series[index], hour, _configuration, _data.Rules);

            if (bar - holding.OpenedBar >= maxHoldBars)
            {
                holding.Ask = Math.Floor(quote?.LowPrice ?? holding.LastLowPrice);
                continue;
            }

            var breakEven = _data.Rules.BreakEvenAsk(holding.UnitCost);
            holding.Ask = quote is not null
                ? Math.Max(quote.Ask, breakEven)
                : Math.Ceiling(breakEven * (1 + _configuration.MinMargin));
        }

        return sellOrders;
    }

    private void FillSells(int bar)
    {
        foreach (var (index, holding) in _holdings.ToList())
        {
            var data = _data.Series[index].Bars[bar];
            if (data.LowVolume > 0)
            {
                holding.LastLowPrice = (decimal)data.LowPrice;
            }

            if (
                holding.Listed <= 0
                || data.HighVolume <= 0
                || (decimal)data.HighPrice < holding.Ask
            )
            {
                continue;
            }

            var quantity = Math.Floor(
                Math.Min(holding.Listed, _settings.ParticipationRate * (decimal)data.HighVolume)
            );
            if (quantity > 0)
            {
                Sell(index, holding, quantity, holding.Ask, bar);
            }
        }
    }

    private void FillBuys(int bar)
    {
        foreach (var (index, order) in _buyOrders.ToList())
        {
            var series = _data.Series[index];
            var data = series.Bars[bar];
            if (data.LowVolume <= 0 || (decimal)data.LowPrice > order.Bid)
            {
                continue;
            }

            var recent = RecentBuys(index, bar);
            var limitLeft = series.BuyLimit - recent.Sum(r => r.Quantity);
            var quantity = Math.Floor(
                Math.Min(
                    order.Remaining,
                    Math.Min(_settings.ParticipationRate * (decimal)data.LowVolume, limitLeft)
                )
            );
            if (quantity <= 0)
            {
                continue;
            }

            recent.Enqueue((bar, quantity));
            if (!_holdings.TryGetValue(index, out var holding))
            {
                holding = new FlipHolding(
                    series.SymbolId,
                    series.Name,
                    _data.TimeOf(bar),
                    bar,
                    order.Bid
                );
                _holdings[index] = holding;
            }

            holding.Buy(quantity, order.Bid);
            order.Remaining -= quantity;
            if (order.Remaining <= 0)
            {
                _buyOrders.Remove(index);
            }
        }
    }

    private Queue<(int Bar, decimal Quantity)> RecentBuys(int index, int bar)
    {
        if (!_recentBuys.TryGetValue(index, out var recent))
        {
            recent = new Queue<(int Bar, decimal Quantity)>();
            _recentBuys[index] = recent;
        }

        var windowBars = FlipQuoter.BuyLimitHours * FlipMarketData.BarsPerHour;
        while (recent.Count > 0 && recent.Peek().Bar <= bar - windowBars)
        {
            recent.Dequeue();
        }

        return recent;
    }

    private void Sell(int index, FlipHolding holding, decimal quantity, decimal price, int bar)
    {
        var net = _data.Rules.NetSale(price, quantity);
        _cash += net;
        holding.Listed = Math.Max(0m, holding.Listed - quantity);

        var closed = holding.Sell(quantity, price, net, _data.TimeOf(bar));
        if (closed is null)
        {
            return;
        }

        _holdings.Remove(index);
        _positions.Add(closed);
    }

    private void CancelBuyOrders()
    {
        foreach (var order in _buyOrders.Values)
        {
            _cash += order.Bid * order.Remaining;
        }

        _buyOrders.Clear();
    }

    private decimal Equity()
    {
        return _cash
            + _buyOrders.Values.Sum(o => o.Bid * o.Remaining)
            + _holdings.Values.Sum(h => h.Quantity * h.LastLowPrice);
    }
}
