using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Executors;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public sealed class BacktestEngine
{
    private const int PriceBucketCount = 25;

    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IEnumerable<ISignalComputer> _signalComputers;
    private readonly Dictionary<StrategyType, IStrategyExecutor> _executors;
    private readonly ILogger<BacktestEngine> _logger;

    public BacktestEngine(
        ILogger<BacktestEngine> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        IEnumerable<IStrategyExecutor> executors,
        CompositeExecutor compositeExecutor,
        IEnumerable<ISignalComputer> signalComputers
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(compositeExecutor);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _signalComputers = signalComputers;
        _executors = executors.ToDictionary(e => e.SupportedType);
        _executors[StrategyType.Composite] = compositeExecutor;
    }

    public async Task<BacktestData> LoadDataAsync(
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var market = await dbContext.Markets.AsNoTracking().FirstAsync(m => m.Id == marketId, cancellationToken);
        var symbols = await dbContext.Symbols
            .AsNoTracking()
            .Where(s => s.MarketId == marketId)
            .ToListAsync(cancellationToken);

        var symbolIds = symbols.Select(s => s.Id).ToList();

        var snapshots = await dbContext.MarketTradeSnapshots
            .AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId) && s.MarketId == marketId)
            .ToListAsync(cancellationToken);

        var forecasts = await dbContext.Forecasts
            .AsNoTracking()
            .Include(f => f.Predictions)
            .Where(f => symbolIds.Contains(f.SymbolId) && f.MarketId == marketId)
            .ToListAsync(cancellationToken);

        var signals = await dbContext.Signals
            .AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId))
            .ToListAsync(cancellationToken);

        var trades = await dbContext.Trades
            .AsNoTracking()
            .Where(t => symbolIds.Contains(t.SymbolId)
                        && t.Timestamp >= startDate
                        && t.Timestamp <= endDate
            )
            .ToListAsync(cancellationToken);

        return new BacktestData(market, symbols, snapshots, forecasts, signals, trades);
    }

    public async Task<BacktestResults> RunAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        CancellationToken cancellationToken,
        StrategyConfiguration? configurationOverride = null,
        BacktestData? data = null
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        data ??= await LoadDataAsync(marketId, startDate, endDate, cancellationToken);

        var configuration = configurationOverride ?? strategy.Configuration;
        var executor = ResolveExecutor(strategy.Type);
        var market = data.Market;
        var symbols = data.Symbols;

        var symbolIds = symbols.Select(s => s.Id).ToList();
        var taxRate = GetTaxRate(market);
        var totalDays = (int)(endDate - startDate).TotalDays;
        var windowDays = DetermineWindowSize(totalDays);

        _logger.LogDebug(
            "Running backtest for strategy '{strategyId}' with {symbolCount} symbols, {totalDays} days, {windowDays}-day windows.",
            strategy.Id,
            symbols.Count,
            totalDays,
            windowDays
        );

        // TODO: Filter snapshots/forecasts by point-in-time once historical versions are supported
        var allSnapshots = data.Snapshots;
        var allForecasts = data.Forecasts;
        var allSignals = data.Signals;

        var state = new BacktestLoopState(budget);

        for (var dayOffset = 0; dayOffset <= totalDays; dayOffset++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentDate = startDate.AddDays(dayOffset);

            CloseExitingPositions(
                state,
                configuration,
                symbols,
                marketId,
                taxRate,
                market,
                executor,
                allSnapshots,
                allSignals,
                allForecasts,
                currentDate,
                data
            );

            var scoredSymbols = await ScoreSymbolsAsync(
                symbols,
                symbolIds,
                marketId,
                market,
                taxRate,
                executor,
                configuration,
                allSnapshots,
                allForecasts,
                currentDate,
                windowDays,
                data,
                cancellationToken
            );

            BuyCandidates(scoredSymbols, configuration, taxRate, state, currentDate, cancellationToken);
            UpdatePortfolioMetrics(state);
        }

        CloseRemainingPositions(state, endDate, taxRate, market, data);
        return ComputeResults(budget, state);
    }

    private void CloseExitingPositions(
        BacktestLoopState state,
        StrategyConfiguration configuration,
        List<Symbol> symbols,
        Id<Market> marketId,
        decimal taxRate,
        Market market,
        IStrategyExecutor executor,
        List<MarketTradeSnapshot> allSnapshots,
        List<Signal> allSignals,
        List<Forecast> allForecasts,
        DateTimeOffset currentDate,
        BacktestData data
    )
    {
        var holdLimit = configuration.HoldPeriodDays ?? int.MaxValue;
        var sellThreshold = configuration.SellThreshold;

        var toClose = new List<KeyValuePair<Id<Symbol>, OpenPosition>>();

        foreach (var kvp in state.OpenPositions)
        {
            if ((currentDate - kvp.Value.EntryTime).Days >= holdLimit)
            {
                toClose.Add(kvp);
            }
        }

        if (sellThreshold.HasValue)
        {
            foreach (var kvp in state.OpenPositions)
            {
                if (toClose.Any(p => p.Key.Equals(kvp.Key)))
                {
                    continue;
                }

                var symbolObj = symbols.First(s => s.Id.Equals(kvp.Value.SymbolId));
                var shouldSell = EvaluateSellSignal(
                    symbolObj,
                    marketId,
                    taxRate,
                    market,
                    executor,
                    configuration,
                    allSnapshots,
                    allSignals,
                    allForecasts,
                    currentDate,
                    data
                );

                if (shouldSell)
                {
                    toClose.Add(kvp);
                }
            }
        }

        foreach (var kvp in toClose)
        {
            var exitPrice = GetClosePrice(kvp.Key, currentDate, data);
            if (exitPrice == 0)
            {
                continue;
            }

            var (netProceeds, exitVolume, netPnl) = ComputeExit(kvp.Value, exitPrice, taxRate, market);
            state.Balance += netProceeds;
            state.ClosedPositions.Add(CreateClosedPosition(kvp.Value, exitPrice, exitVolume, netPnl, currentDate));
            state.OpenPositions.Remove(kvp.Key);
        }
    }

    private async Task<List<(Symbol Symbol, decimal Score, decimal Price)>> ScoreSymbolsAsync(
        List<Symbol> symbols,
        List<Id<Symbol>> symbolIds,
        Id<Market> marketId,
        Market market,
        decimal taxRate,
        IStrategyExecutor executor,
        StrategyConfiguration configuration,
        List<MarketTradeSnapshot> allSnapshots,
        List<Forecast> allForecasts,
        DateTimeOffset currentDate,
        int windowDays,
        BacktestData data,
        CancellationToken cancellationToken
    )
    {
        // Exclude current day to avoid look-ahead bias
        var windowStart = currentDate.AddDays(-windowDays);
        var previousClose = currentDate;

        var windowTradesRaw = data.Trades
            .Where(t => symbolIds.Contains(t.SymbolId)
                        && t.Timestamp >= windowStart
                        && t.Timestamp < previousClose
            )
            .ToList();

        var windowTrades = windowTradesRaw
            .GroupBy(t => t.SymbolId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var scored = new List<(Symbol Symbol, decimal Score, decimal Price)>();

        foreach (var symbol in symbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!windowTrades.TryGetValue(symbol.Id, out var trades) || trades.Count == 0)
            {
                continue;
            }

            var currentPrice = trades.MaxBy(t => t.Timestamp)?.Price ?? 0;
            if (currentPrice == 0)
            {
                continue;
            }

            var limit = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
            var snapshots = GetSnapshotsForSymbol(symbol.Id, allSnapshots);
            var priceBuckets = BuildPriceBuckets(trades);
            var forecast = allForecasts.FirstOrDefault(f => f.SymbolId.Equals(symbol.Id));
            var (forecastedPrice, forecastedChange) = GetForecastData(forecast, currentPrice);

            var signals = await ReconstructSignalsAsync(
                symbol.Id,
                taxRate,
                limit,
                snapshots,
                priceBuckets,
                cancellationToken
            );

            var context = new StrategyScoreContext(
                symbol.Id,
                marketId,
                symbol.Name,
                symbol.Subcode,
                currentPrice,
                taxRate,
                limit,
                snapshots.Short,
                snapshots.Medium,
                snapshots.Long,
                priceBuckets,
                signals,
                forecastedPrice,
                forecastedChange
            );

            var score = executor.Score(context, configuration);
            if (score is not null)
            {
                scored.Add((symbol, score.Value, currentPrice));
            }
        }

        return scored;
    }

    internal static void BuyCandidates(
        List<(Symbol Symbol, decimal Score, decimal Price)> scoredSymbols,
        StrategyConfiguration configuration,
        decimal taxRate,
        BacktestLoopState state,
        DateTimeOffset currentDate,
        CancellationToken cancellationToken
    )
    {
        var maxPositions = configuration.MaxPositions ?? int.MaxValue;
        var maxPositionPercent = configuration.MaxPositionPercent ?? 1m;
        var buyThreshold = configuration.BuyThreshold ?? 0.1m;

        var buyCandidates = scoredSymbols
            .Where(s => s.Score > buyThreshold && !state.OpenPositions.ContainsKey(s.Symbol.Id))
            .OrderByDescending(s => s.Score)
            .Take(maxPositions - state.OpenPositions.Count)
            .ToList();

        foreach (var candidate in buyCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var maxPositionBudget = state.Balance * maxPositionPercent;
            var costPerUnit = candidate.Price;
            var buyingPower = maxPositionBudget / (costPerUnit * (1 + taxRate));
            var volume = Math.Floor(buyingPower);

            if (volume < 1 || costPerUnit == 0)
            {
                continue;
            }

            var cost = costPerUnit * volume * (1 + taxRate);
            if (cost > state.Balance)
            {
                continue;
            }

            state.Balance -= cost;
            state.OpenPositions[candidate.Symbol.Id] = new OpenPosition(
                candidate.Symbol.Id,
                candidate.Symbol.Name,
                candidate.Symbol.Subcode,
                costPerUnit,
                volume,
                currentDate
            );
        }
    }

    internal static void UpdatePortfolioMetrics(BacktestLoopState state)
    {
        var openPositionValue = state.OpenPositions.Values.Sum(p => p.EntryPrice * p.Volume);
        var currentPortfolioValue = state.Balance + openPositionValue;
        state.PeakPortfolioValue = Math.Max(state.PeakPortfolioValue, currentPortfolioValue);

        var drawdown = state.PeakPortfolioValue > 0
            ? (state.PeakPortfolioValue - currentPortfolioValue) / state.PeakPortfolioValue
            : 0;
        state.MaxDrawdown = Math.Max(state.MaxDrawdown, drawdown);
        state.PortfolioValues.Add(currentPortfolioValue);
    }

    private void CloseRemainingPositions(
        BacktestLoopState state,
        DateTimeOffset endDate,
        decimal taxRate,
        Market market,
        BacktestData data
    )
    {
        foreach (var pos in state.OpenPositions.Values.ToList())
        {
            var exitPrice = GetClosePrice(pos.SymbolId, endDate, data);
            if (exitPrice == 0)
            {
                exitPrice = pos.EntryPrice;
            }

            var (netProceeds, exitVolume, netPnl) = ComputeExit(pos, exitPrice, taxRate, market);
            state.Balance += netProceeds;
            state.ClosedPositions.Add(CreateClosedPosition(pos, exitPrice, exitVolume, netPnl, endDate));
        }
    }

    private IStrategyExecutor ResolveExecutor(StrategyType type)
    {
        if (!_executors.TryGetValue(type, out var executor))
        {
            throw new InvalidOperationException($"No executor registered for strategy type '{type}'.");
        }

        return executor;
    }

    private bool EvaluateSellSignal(
        Symbol symbol,
        Id<Market> marketId,
        decimal taxRate,
        Market market,
        IStrategyExecutor executor,
        StrategyConfiguration configuration,
        List<MarketTradeSnapshot> allSnapshots,
        List<Signal> allSignals,
        List<Forecast> allForecasts,
        DateTimeOffset currentDate,
        BacktestData data
    )
    {
        var sellThreshold = configuration.SellThreshold!.Value;
        var snapshots = GetSnapshotsForSymbol(symbol.Id, allSnapshots);
        var symbolSignals = GetSignalsForSymbol(symbol.Id, allSignals);
        var forecast = allForecasts.FirstOrDefault(f => f.SymbolId.Equals(symbol.Id));

        var currentPrice = GetClosePrice(symbol.Id, currentDate, data);
        if (currentPrice == 0)
        {
            return false;
        }

        var limit = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var (forecastedPrice, forecastedChange) = GetForecastData(forecast, currentPrice);

        var context = new StrategyScoreContext(
            symbol.Id,
            marketId,
            symbol.Name,
            symbol.Subcode,
            currentPrice,
            taxRate,
            limit,
            snapshots.Short,
            snapshots.Medium,
            snapshots.Long,
            [],
            symbolSignals,
            forecastedPrice,
            forecastedChange
        );

        var score = executor.Score(context, configuration);
        return score < sellThreshold;
    }

    internal static int DetermineWindowSize(int totalDays)
    {
        if (totalDays <= 30)
        {
            return 1;
        }

        if (totalDays <= 90)
        {
            return 3;
        }

        if (totalDays <= 365)
        {
            return 7;
        }

        return 14;
    }

    internal static decimal GetTaxRate(Market market)
    {
        return market.Taxes.Flat?.Rate ?? 0m;
    }

    internal static (decimal NetProceeds, decimal ExitVolume, decimal NetPnl) ComputeExit(
        OpenPosition pos,
        decimal exitPrice,
        decimal taxRate,
        Market market
    )
    {
        var exitVolume = pos.Volume;
        var grossExitValue = exitPrice * exitVolume;
        var taxAmount = grossExitValue * taxRate;
        var taxCap = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var cappedTax = Math.Min(taxAmount, taxCap);

        var netProceeds = grossExitValue - cappedTax;
        var costBasis = pos.EntryPrice * exitVolume;
        var netPnl = netProceeds - costBasis;

        return (netProceeds, exitVolume, netPnl);
    }

    private static decimal GetClosePrice(Id<Symbol> symbolId, DateTimeOffset date, BacktestData data)
    {
        var price = data.Trades
            .Where(t => t.SymbolId == symbolId && t.Timestamp <= date)
            .MaxBy(t => t.Timestamp)?.Price ?? 0m;
        return price;
    }

    internal static (MarketTradeSnapshot? Short, MarketTradeSnapshot? Medium, MarketTradeSnapshot? Long)
        GetSnapshotsForSymbol(Id<Symbol> symbolId, List<MarketTradeSnapshot> allSnapshots)
    {
        var symbolSnaps = allSnapshots.Where(s => s.SymbolId.Equals(symbolId)).ToList();
        return (
            symbolSnaps.FirstOrDefault(s => s.TimeFrame == TimeFrame.OneHour),
            symbolSnaps.FirstOrDefault(s => s.TimeFrame == TimeFrame.OneWeek),
            symbolSnaps.FirstOrDefault(s => s.TimeFrame == TimeFrame.OneMonth)
        );
    }

    internal static List<Signal> GetSignalsForSymbol(Id<Symbol> symbolId, List<Signal> allSignals)
    {
        return allSignals.Where(s => s.SymbolId.Equals(symbolId)).ToList();
    }

    internal static (decimal? Price, decimal? Change) GetForecastData(Forecast? forecast, decimal currentPrice)
    {
        if (forecast is null || currentPrice == 0)
        {
            return (null, null);
        }

        var forecastedPrice = forecast.Latest.AveragePrice;
        if (forecastedPrice == 0)
        {
            return (forecastedPrice, null);
        }

        return (forecastedPrice, (forecastedPrice - currentPrice) / currentPrice);
    }

    internal static IReadOnlyList<PriceBucket> BuildPriceBuckets(List<Trade> trades)
    {
        if (trades.Count == 0)
        {
            return [];
        }

        var sorted = trades.OrderBy(t => t.Timestamp).ToList();
        var bucketSize = Math.Max(1, sorted.Count / PriceBucketCount);
        var buckets = new List<PriceBucket>();

        for (var i = 0; i < sorted.Count; i += bucketSize)
        {
            var remaining = Math.Min(bucketSize, sorted.Count - i);
            var chunk = sorted.GetRange(i, remaining);

            buckets.Add(
                new PriceBucket(
                    chunk[0].Timestamp,
                    chunk.Average(t => t.Price),
                    chunk.Min(t => t.Price),
                    chunk.Max(t => t.Price),
                    chunk.Sum(t => t.Volume)
                )
            );
        }

        return buckets;
    }

    private async Task<IReadOnlyList<Signal>> ReconstructSignalsAsync(
        Id<Symbol> symbolId,
        decimal taxRate,
        decimal limit,
        (MarketTradeSnapshot? Short, MarketTradeSnapshot? Medium, MarketTradeSnapshot? Long) snapshots,
        IReadOnlyList<PriceBucket> priceBuckets,
        CancellationToken ct
    )
    {
        var signals = new List<Signal>();

        foreach (var computer in _signalComputers)
        {
            var context = new SignalComputeContext(
                symbolId,
                snapshots.Short?.MarketId ?? default,
                taxRate,
                limit,
                snapshots.Short,
                snapshots.Medium,
                snapshots.Long,
                priceBuckets
            );

            var value = await computer.ComputeAsync(context, ct);
            if (value is not null)
            {
                signals.Add(Signal.Create(default, symbolId, computer.Type, value.Value));
            }
        }

        return signals;
    }

    internal static BacktestPosition CreateClosedPosition(
        OpenPosition pos,
        decimal exitPrice,
        decimal exitVolume,
        decimal profitLoss,
        DateTimeOffset exitTime
    )
    {
        var returnPercent = pos.EntryPrice > 0 ? profitLoss / (pos.EntryPrice * exitVolume) : 0;

        return new BacktestPosition
        {
            SymbolId = pos.SymbolId.ToString(),
            SymbolName = pos.SymbolName,
            EntryPrice = pos.EntryPrice,
            ExitPrice = exitPrice,
            Volume = exitVolume,
            ProfitLoss = profitLoss,
            ReturnPercent = returnPercent,
            EntryTime = pos.EntryTime,
            ExitTime = exitTime
        };
    }

    internal static BacktestResults ComputeResults(decimal budget, BacktestLoopState state)
    {
        var totalReturn = state.Balance - budget;
        var totalReturnPercent = budget > 0 ? totalReturn / budget : 0;
        var winningTrades = state.ClosedPositions.Count(p => p.ProfitLoss > 0);
        var losingTrades = state.ClosedPositions.Count(p => p.ProfitLoss <= 0);
        var winRate = state.ClosedPositions.Count > 0 ? (decimal)winningTrades / state.ClosedPositions.Count : 0;
        var sharpeRatio = ComputeSharpeRatio(state.PortfolioValues);

        return new BacktestResults
        {
            TotalReturn = totalReturn,
            TotalReturnPercent = totalReturnPercent,
            MaxDrawdown = state.MaxDrawdown * budget,
            MaxDrawdownPercent = state.MaxDrawdown,
            WinRate = winRate,
            TotalTrades = state.ClosedPositions.Count,
            WinningTrades = winningTrades,
            LosingTrades = losingTrades,
            SharpeRatio = sharpeRatio,
            AverageTradeReturn =
                state.ClosedPositions.Count > 0 ? state.ClosedPositions.Average(p => p.ProfitLoss) : 0,
            BestTrade = state.ClosedPositions.Count > 0 ? state.ClosedPositions.Max(p => p.ProfitLoss) : 0,
            WorstTrade = state.ClosedPositions.Count > 0 ? state.ClosedPositions.Min(p => p.ProfitLoss) : 0,
            FinalBalance = state.Balance,
            Positions = state.ClosedPositions
        };
    }

    internal static decimal ComputeSharpeRatio(List<decimal> portfolioValues)
    {
        if (portfolioValues.Count <= 2)
        {
            return 0m;
        }

        var returns = new List<decimal>();
        for (var i = 1; i < portfolioValues.Count; i++)
        {
            if (portfolioValues[i - 1] != 0)
            {
                returns.Add((portfolioValues[i] - portfolioValues[i - 1]) / portfolioValues[i - 1]);
            }
        }

        if (returns.Count == 0)
        {
            return 0m;
        }

        var avgReturn = returns.Average();
        var variance = returns.Average(r => (r - avgReturn) * (r - avgReturn));
        var stdDev = (decimal)Math.Sqrt((double)variance);
        return stdDev > 0 ? avgReturn / stdDev * (decimal)Math.Sqrt(365) : 0;
    }
}
