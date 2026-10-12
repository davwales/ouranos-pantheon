using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Trading;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <summary>
///     Simulates a trader who acts once a day. Each day, positions past their hold period or
///     scoring below the sell threshold are sold, then the best-scoring symbols above the buy
///     threshold are bought until the position limit is reached. All fills happen at the day's
///     average price, capped by the participation share of its volume and moved by square-root
///     slippage. Inventory left at the end is sold, and what liquidity cannot absorb is marked at
///     the day's lowest price.
/// </summary>
public sealed class SwingSimulator
{
    private const int ProgressSteps = 20;

    private static readonly IReadOnlyDictionary<Id<Symbol>, SwingInputs> NoInputs =
        new Dictionary<Id<Symbol>, SwingInputs>();

    private readonly SwingMarketData _market;
    private readonly IReadOnlyDictionary<
        DateOnly,
        IReadOnlyDictionary<Id<Symbol>, SwingInputs>
    > _inputs;
    private readonly SwingConfiguration _configuration;
    private readonly SimulationSettings _settings;
    private readonly BacktestDataOptions _options;
    private readonly Dictionary<Id<Symbol>, Holding> _holdings = [];
    private readonly List<BacktestPosition> _positions = [];
    private readonly EquityCurve _equity;
    private decimal _cash;

    private SwingSimulator(
        SwingData data,
        SwingConfiguration configuration,
        SimulationSettings settings,
        BacktestDataOptions options
    )
    {
        _market = data.Market;
        _inputs = data.Inputs(Day(settings.Start), Day(settings.Start.AddDays(settings.TotalDays)));
        _configuration = configuration;
        _settings = settings;
        _options = options;
        _cash = settings.Budget;
        _equity = new EquityCurve(settings.Budget);
    }

    public static SimulationResult Run(
        SwingData data,
        SwingConfiguration configuration,
        SimulationSettings settings,
        BacktestDataOptions options
    )
    {
        Guard.Against.Null(data);
        Guard.Against.Null(configuration);
        Guard.Against.Null(settings);
        Guard.Against.Null(options);

        return new SwingSimulator(data, configuration, settings, options).Simulate();
    }

    /// <summary>
    ///     Walks <paramref name="candidates" /> best first and sizes each from the cash still
    ///     unspent, stopping at the position limit. Shared by the simulator and live
    ///     recommendations so both pick and size symbols the same way.
    /// </summary>
    public static IEnumerable<SwingEntry> PlanEntries(
        IEnumerable<SwingCandidate> candidates,
        SwingConfiguration configuration,
        Func<Id<Symbol>, bool> isHeld,
        int heldCount,
        decimal cash,
        Func<SwingCandidate, decimal, SwingEntry?> size
    )
    {
        var buyThreshold = configuration.BuyThreshold ?? 0m;
        var maxPositions = configuration.MaxPositions ?? int.MaxValue;
        var positionShare = Math.Min(configuration.MaxPositionPercent ?? 1m, 1m);

        var ranked = candidates
            .Where(c => c.Score > buyThreshold && !isHeld(c.Symbol.Id))
            .OrderByDescending(c => c.Score);

        foreach (var candidate in ranked)
        {
            if (heldCount >= maxPositions)
            {
                yield break;
            }

            var entry = size(candidate, cash * positionShare);
            if (entry is null)
            {
                continue;
            }

            cash -= entry.Cost;
            heldCount++;
            yield return entry;
        }
    }

    private static DateOnly Day(DateTimeOffset date)
    {
        return DateOnly.FromDateTime(date.UtcDateTime);
    }

    private SimulationResult Simulate()
    {
        var totalDays = _settings.TotalDays;
        var progressInterval = Math.Max(1, totalDays / ProgressSteps);

        for (var i = 0; i <= totalDays; i++)
        {
            _settings.CancellationToken.ThrowIfCancellationRequested();
            if (i % progressInterval == 0)
            {
                _settings.Progress?.Report((double)i / (totalDays + 1));
            }

            var date = _settings.Start.AddDays(i);
            var inputs = _inputs.GetValueOrDefault(Day(date)) ?? NoInputs;
            var candidates = Score(inputs);

            CloseExits(date, inputs);
            OpenEntries(date, candidates);
            _equity.Record(Equity(date));
        }

        Liquidate(_settings.End);
        _settings.Progress?.Report(1);

        return new SimulationResult(_positions, _equity, _cash);
    }

    private List<SwingCandidate> Score(IReadOnlyDictionary<Id<Symbol>, SwingInputs> inputs)
    {
        var candidates = new List<SwingCandidate>();
        foreach (var symbol in _market.Symbols)
        {
            if (!inputs.TryGetValue(symbol.Id, out var symbolInputs) || !symbolInputs.IsEligible)
            {
                continue;
            }

            var score = SwingScoring.Combine(symbolInputs.Scores, _configuration.InputWeights);
            if (score.HasValue)
            {
                candidates.Add(new SwingCandidate(symbol, score.Value, symbolInputs.Price));
            }
        }

        return candidates;
    }

    private void CloseExits(
        DateTimeOffset date,
        IReadOnlyDictionary<Id<Symbol>, SwingInputs> inputs
    )
    {
        var holdLimit = _configuration.HoldPeriodDays ?? int.MaxValue;

        var toClose = _holdings
            .Where(h => (date - h.Value.OpenedAt).Days >= holdLimit)
            .Select(h => h.Key)
            .ToList();

        if (_configuration.SellThreshold is { } sellThreshold)
        {
            foreach (var symbolId in _holdings.Keys)
            {
                if (toClose.Contains(symbolId) || !inputs.TryGetValue(symbolId, out var held))
                {
                    continue;
                }

                if (SwingScoring.Combine(held.Scores, _configuration.InputWeights) < sellThreshold)
                {
                    toClose.Add(symbolId);
                }
            }
        }

        foreach (var symbolId in toClose)
        {
            var day = _market.GetDailyAggregate(symbolId, date);
            if (day is null || day.AveragePrice <= 0)
            {
                continue;
            }

            Sell(_holdings[symbolId], day, date);
        }
    }

    private void OpenEntries(DateTimeOffset date, List<SwingCandidate> candidates)
    {
        var entries = PlanEntries(
            candidates,
            _configuration,
            _holdings.ContainsKey,
            _holdings.Count,
            _cash,
            (candidate, budget) => Size(candidate, budget, date)
        );

        foreach (var entry in entries)
        {
            var symbol = entry.Candidate.Symbol;
            var holding = new Holding(symbol.Id, symbol.Name, date);
            holding.Buy(entry.Volume, entry.Price);
            _cash -= entry.Cost;
            _holdings[symbol.Id] = holding;
        }
    }

    private SwingEntry? Size(SwingCandidate candidate, decimal budget, DateTimeOffset date)
    {
        var day = _market.GetDailyAggregate(candidate.Symbol.Id, date);
        var buyLimit = candidate.Symbol.AdditionalFields.Limit;

        if (day is null || day.AveragePrice <= 0 || buyLimit == 0)
        {
            return null;
        }

        var size = PositionSizer.Size(
            day.AveragePrice,
            budget,
            _market.GetFillCap(candidate.Symbol.Id, date, _settings.ParticipationRate),
            buyLimit,
            _options.LimitWindowsPerDay,
            _options.MinFillFraction
        );

        if (size.Volume < 1)
        {
            return null;
        }

        var slippage = SwingMath.ComputeSlippage(
            size.Volume,
            day.TotalVolume,
            _settings.SlippageMultiplier
        );

        var price = day.AveragePrice * (1 + slippage);
        var volume = Math.Min(size.Volume, Math.Floor(budget / price));
        return volume < 1 ? null : new SwingEntry(candidate, volume, price, size.LimitedBy);
    }

    private decimal Equity(DateTimeOffset date)
    {
        return _cash
            + _holdings.Values.Sum(h => _market.GetLatestPrice(h.SymbolId, date) * h.Quantity);
    }

    /// <summary>Returns whether the holding sold out.</summary>
    private bool Sell(Holding holding, DailyTradeAggregate day, DateTimeOffset date)
    {
        var (netProceeds, volume) = SwingMath.ComputeExit(
            holding.Quantity,
            day.AveragePrice,
            _market.Rules,
            day.TotalVolume,
            _market.GetFillCap(holding.SymbolId, date, _settings.ParticipationRate),
            _settings.SlippageMultiplier
        );

        if (volume <= 0)
        {
            return false;
        }

        _cash += netProceeds;
        return Close(holding, holding.Sell(volume, day.AveragePrice, netProceeds, date));
    }

    private bool Close(Holding holding, BacktestPosition? closed)
    {
        if (closed is null)
        {
            return false;
        }

        _positions.Add(closed);
        _holdings.Remove(holding.SymbolId);
        return true;
    }

    private void Liquidate(DateTimeOffset end)
    {
        foreach (var holding in _holdings.Values.ToList())
        {
            var day = _market.GetDailyAggregate(holding.SymbolId, end);

            if (day is not null && day.AveragePrice > 0 && Sell(holding, day, end))
            {
                continue;
            }

            MarkToMarket(holding, end);
        }
    }

    /// <summary>
    ///     Volume the liquidity cap could not sell by the end of the run is valued at the latest
    ///     day's minimum traded price (net of tax) rather than the average, so stranded inventory
    ///     in thin symbols cannot inflate results.
    /// </summary>
    private void MarkToMarket(Holding holding, DateTimeOffset end)
    {
        var latest = _market.GetLatestAggregate(holding.SymbolId, end);
        var markPrice = latest is { MinPrice: > 0 } ? latest.MinPrice : holding.UnitCost;
        var netProceeds = _market.Rules.NetSale(markPrice, holding.Quantity);

        _cash += netProceeds;
        Close(holding, holding.Sell(holding.Quantity, markPrice, netProceeds, end));
    }
}
