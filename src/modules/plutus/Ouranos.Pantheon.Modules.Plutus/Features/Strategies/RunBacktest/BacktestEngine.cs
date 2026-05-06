using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Domain;
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

    private readonly IBacktestDataQueryService _dataService;
    private readonly IEnumerable<ISignalComputer> _signalComputers;
    private readonly Dictionary<StrategyType, IStrategyExecutor> _executors;
    private readonly ILogger<BacktestEngine> _logger;

    public BacktestEngine(
        ILogger<BacktestEngine> logger,
        IBacktestDataQueryService dataService,
        IEnumerable<IStrategyExecutor> executors,
        CompositeExecutor compositeExecutor,
        IEnumerable<ISignalComputer> signalComputers
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dataService);
        Guard.Against.Null(compositeExecutor);

        _logger = logger;
        _dataService = dataService;
        _signalComputers = signalComputers;
        _executors = executors.ToDictionary(e => e.SupportedType);
        _executors[StrategyType.Composite] = compositeExecutor;
    }

    public async Task<BacktestResults> RunAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        CancellationToken cancellationToken,
        StrategyConfiguration? configurationOverride = null,
        BacktestData? data = null,
        Func<int, string, Task>? onCheckpoint = null,
        decimal volumeParticipationRate = 0.25m,
        decimal slippageMultiplier = 0.1m
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configuration = configurationOverride ?? strategy.Configuration;
        var executor = ResolveExecutor(strategy.Type);
        var totalDays = (int)(endDate - startDate).TotalDays;
        var windowDays = DetermineWindowSize(totalDays);

        data ??= await _dataService.LoadDataAsync(
            marketId,
            startDate,
            endDate,
            cancellationToken,
            lookbackDays: 30
        );

        var market = data.Market;
        var symbols = data.Symbols;
        var taxRate = GetTaxRate(market);

        _logger.LogDebug(
            "Running backtest for strategy '{strategyId}' with {symbolCount} symbols, {totalDays} days, {windowDays}-day windows.",
            strategy.Id,
            symbols.Count,
            totalDays,
            windowDays
        );

        _logger.LogDebug(
            "Backtest data loaded: {aggregateSymbolCount} symbols with aggregates, {dailyPriceCount} daily prices, " +
            "{snapshotSymbolCount} symbols with snapshots, {forecastSymbolCount} symbols with forecasts.",
            data.AggregatesBySymbol.Count,
            data.DailyPricesByDate.Count,
            data.SnapshotsBySymbol.Count,
            data.ForecastBySymbol.Count
        );

        if (onCheckpoint is not null)
        {
            await onCheckpoint(5, "Market data loaded, starting simulation...");
        }

        var state = new BacktestLoopState(budget);

        var progressInterval = Math.Max(1, totalDays / 20);

        for (var dayOffset = 0; dayOffset <= totalDays; dayOffset++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentDate = startDate.AddDays(dayOffset);

            if (dayOffset % progressInterval == 0 || dayOffset == totalDays)
            {
                var percent = 10 + (int)(80.0 * dayOffset / totalDays);
                if (onCheckpoint is not null)
                {
                    await onCheckpoint(
                        Math.Min(percent, 90),
                        $"Simulating day {dayOffset} of {totalDays}..."
                    );
                }
            }

            await CloseExitingPositionsAsync(
                state,
                configuration,
                marketId,
                taxRate,
                market,
                executor,
                currentDate,
                data,
                volumeParticipationRate,
                slippageMultiplier,
                cancellationToken
            );

            var scoredSymbols = await ScoreSymbolsAsync(
                symbols,
                marketId,
                taxRate,
                executor,
                configuration,
                currentDate,
                data,
                cancellationToken
            );

            BuyCandidates(
                scoredSymbols,
                configuration,
                taxRate,
                state,
                currentDate,
                data,
                volumeParticipationRate,
                cancellationToken
            );
            UpdatePortfolioMetrics(state, currentDate, data);

            if (dayOffset == totalDays || dayOffset == 0)
            {
                _logger.LogDebug(
                    "Day {currentDate}: {openPositionCount} open positions, {balance:F2} balance, " +
                    "{buyThreshold} buy threshold, {candidateCount} buy candidates.",
                    currentDate,
                    state.OpenPositions.Count,
                    state.Balance,
                    configuration.BuyThreshold ?? 0m,
                    scoredSymbols.Count(s => s.Score > (configuration.BuyThreshold ?? 0m))
                );
            }
        }

        if (onCheckpoint is not null)
        {
            await onCheckpoint(95, "Closing remaining positions...");
        }

        CloseRemainingPositions(state, endDate, taxRate, market, data, volumeParticipationRate, slippageMultiplier);

        if (onCheckpoint is not null)
        {
            await onCheckpoint(99, "Computing results...");
        }

        return ComputeResults(budget, state);
    }

    private async Task CloseExitingPositionsAsync(
        BacktestLoopState state,
        StrategyConfiguration configuration,
        Id<Market> marketId,
        decimal taxRate,
        Market market,
        IStrategyExecutor executor,
        DateTimeOffset currentDate,
        BacktestData data,
        decimal volumeParticipationRate,
        decimal slippageMultiplier,
        CancellationToken cancellationToken
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

                var shouldSell = await EvaluateSellSignalAsync(
                    kvp.Value.SymbolId,
                    kvp.Value.SymbolName,
                    kvp.Value.SymbolSubcode,
                    marketId,
                    taxRate,
                    market,
                    executor,
                    configuration,
                    currentDate,
                    data,
                    cancellationToken
                );

                if (shouldSell)
                {
                    toClose.Add(kvp);
                }
            }
        }

        foreach (var kvp in toClose)
        {
            var exitPrice = data.GetLatestPrice(kvp.Key, currentDate);

            if (exitPrice == 0)
            {
                continue;
            }

            var dailyVolume = data.GetDailyVolume(kvp.Key, currentDate);
            var (netProceeds, exitVolume, netPnl) = ComputeExit(
                kvp.Value,
                exitPrice,
                taxRate,
                market,
                dailyVolume,
                volumeParticipationRate,
                slippageMultiplier
            );

            if (exitVolume <= 0)
            {
                continue;
            }

            state.Balance += netProceeds;
            state.ClosedPositions.Add(CreateClosedPosition(kvp.Value, exitPrice, exitVolume, netPnl, currentDate));

            if (exitVolume >= kvp.Value.Volume)
            {
                state.OpenPositions.Remove(kvp.Key);
            }
            else
            {
                state.OpenPositions[kvp.Key] = kvp.Value with { Volume = kvp.Value.Volume - exitVolume };
            }
        }
    }

    private async Task<List<(Symbol Symbol, decimal Score, decimal Price)>> ScoreSymbolsAsync(
        List<Symbol> symbols,
        Id<Market> marketId,
        decimal taxRate,
        IStrategyExecutor executor,
        StrategyConfiguration configuration,
        DateTimeOffset currentDate,
        BacktestData data,
        CancellationToken cancellationToken
    )
    {
        var scored = new List<(Symbol Symbol, decimal Score, decimal Price)>();

        var skippedNoAggregates = 0;
        var skippedZeroPrice = 0;
        var scoredNull = 0;

        foreach (var symbol in symbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var allAggregates = data.GetWindowAggregates(symbol.Id, DateTimeOffset.MinValue, currentDate);

            if (allAggregates.Count == 0)
            {
                skippedNoAggregates++;
                continue;
            }

            var currentPrice = allAggregates.MaxBy(a => a.Date)?.AveragePrice ?? 0;
            if (currentPrice == 0)
            {
                skippedZeroPrice++;
                continue;
            }

            var limit = data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
            var snapshots = data.GetSnapshotsForSymbol(symbol.Id);
            var priceBuckets = BuildPriceBucketsFromAggregates(allAggregates);
            var forecast = data.GetForecastForSymbol(symbol.Id);
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
            else
            {
                scoredNull++;
            }
        }

        _logger.LogDebug(
            "Day {currentDate}: scored {scoredCount}/{symbolCount} symbols. " +
            "Skipped: {skippedNoAggregates} no aggregates, {skippedZeroPrice} zero price, {scoredNull} null score.",
            currentDate,
            scored.Count,
            symbols.Count,
            skippedNoAggregates,
            skippedZeroPrice,
            scoredNull
        );

        return scored;
    }

    internal static void BuyCandidates(
        List<(Symbol Symbol, decimal Score, decimal Price)> scoredSymbols,
        StrategyConfiguration configuration,
        decimal taxRate,
        BacktestLoopState state,
        DateTimeOffset currentDate,
        BacktestData data,
        decimal volumeParticipationRate,
        CancellationToken cancellationToken
    )
    {
        var maxPositions = configuration.MaxPositions ?? int.MaxValue;
        var maxPositionPercent = configuration.MaxPositionPercent ?? 1m;
        var buyThreshold = configuration.BuyThreshold ?? 0m;

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

            var symbolLimit = candidate.Symbol.AdditionalFields.Limit;
            if (volume > symbolLimit)
            {
                volume = symbolLimit.Value;
            }

            var dailyVolume = data.GetDailyVolume(candidate.Symbol.Id, currentDate);
            if (dailyVolume > 0)
            {
                var maxBuyableVolume = Math.Floor(dailyVolume * volumeParticipationRate);
                volume = Math.Min(volume, maxBuyableVolume);
            }

            if (volume < 1)
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

    internal static void UpdatePortfolioMetrics(
        BacktestLoopState state,
        DateTimeOffset currentDate,
        BacktestData data
    )
    {
        var openPositionValue = state.OpenPositions.Values
            .Sum(p => data.GetLatestPrice(p.SymbolId, currentDate) * p.Volume);
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
        BacktestData data,
        decimal volumeParticipationRate,
        decimal slippageMultiplier
    )
    {
        foreach (var pos in state.OpenPositions.Values.ToList())
        {
            var exitPrice = data.GetLatestPrice(pos.SymbolId, endDate);
            if (exitPrice == 0)
            {
                exitPrice = pos.EntryPrice;
            }

            var dailyVolume = data.GetDailyVolume(pos.SymbolId, endDate);
            var (netProceeds, exitVolume, netPnl) = ComputeExit(
                pos,
                exitPrice,
                taxRate,
                market,
                dailyVolume,
                volumeParticipationRate,
                slippageMultiplier
            );

            if (exitVolume <= 0)
            {
                var forcedExitPrice = pos.EntryPrice * 0.5m;
                var forcedGrossValue = forcedExitPrice * pos.Volume;
                var forcedTax = forcedGrossValue * taxRate;
                var forcedTaxCap = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
                var forcedCappedTax = Math.Min(forcedTax, forcedTaxCap);
                var forcedNetProceeds = forcedGrossValue - forcedCappedTax;

                state.Balance += forcedNetProceeds;
                state.ClosedPositions.Add(
                    CreateClosedPosition(
                        pos,
                        forcedExitPrice,
                        pos.Volume,
                        forcedNetProceeds - pos.EntryPrice * pos.Volume,
                        endDate
                    )
                );
                continue;
            }

            state.Balance += netProceeds;
            state.ClosedPositions.Add(CreateClosedPosition(pos, exitPrice, exitVolume, netPnl, endDate));

            if (exitVolume < pos.Volume)
            {
                var remainingVolume = pos.Volume - exitVolume;
                var remainingCostBasis = pos.EntryPrice * remainingVolume;
                var forcedExitPrice2 = pos.EntryPrice * 0.5m;
                var forcedGrossValue2 = forcedExitPrice2 * remainingVolume;
                var forcedTax2 = forcedGrossValue2 * taxRate;
                var forcedTaxCap2 = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
                var forcedNetProceeds2 = forcedGrossValue2 - Math.Min(forcedTax2, forcedTaxCap2);

                state.Balance += forcedNetProceeds2;
                state.ClosedPositions.Add(
                    CreateClosedPosition(
                        pos with { Volume = remainingVolume },
                        forcedExitPrice2,
                        remainingVolume,
                        forcedNetProceeds2 - remainingCostBasis,
                        endDate
                    )
                );
            }
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

    /// <summary>
    ///     Evaluates the sell signal for an open position by reconstructing signals
    ///     from the window data, ensuring consistency with the buy path.
    ///     This fixes Bug 3: previously the sell path used stale live signals from the DB,
    ///     while the buy path reconstructed from window data — producing different scores.
    /// </summary>
    private async Task<bool> EvaluateSellSignalAsync(
        Id<Symbol> symbolId,
        string symbolName,
        string? symbolSubcode,
        Id<Market> marketId,
        decimal taxRate,
        Market market,
        IStrategyExecutor executor,
        StrategyConfiguration configuration,
        DateTimeOffset currentDate,
        BacktestData data,
        CancellationToken cancellationToken
    )
    {
        var sellThreshold = configuration.SellThreshold!.Value;
        var snapshots = data.GetSnapshotsForSymbol(symbolId);
        var currentPrice = data.GetLatestPrice(symbolId, currentDate);
        if (currentPrice == 0)
        {
            return false;
        }

        var limit = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var forecast = data.GetForecastForSymbol(symbolId);
        var (forecastedPrice, forecastedChange) = GetForecastData(forecast, currentPrice);

        var allAggregates = data.GetWindowAggregates(symbolId, DateTimeOffset.MinValue, currentDate);
        var priceBuckets = BuildPriceBucketsFromAggregates(allAggregates);

        var signals = await ReconstructSignalsAsync(
            symbolId,
            taxRate,
            limit,
            snapshots,
            priceBuckets,
            cancellationToken
        );

        var context = new StrategyScoreContext(
            symbolId,
            marketId,
            symbolName,
            symbolSubcode,
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
        Market market,
        decimal dailyVolume,
        decimal volumeParticipationRate,
        decimal slippageMultiplier
    )
    {
        var maxSellableVolume = dailyVolume > 0
            ? Math.Floor(dailyVolume * volumeParticipationRate)
            : pos.Volume;

        var exitVolume = Math.Min(pos.Volume, maxSellableVolume);

        if (exitVolume <= 0)
        {
            return (0m, 0m, 0m);
        }

        var volumeImpact = dailyVolume > 0
            ? pos.Volume / dailyVolume
            : 0m;
        var slippage = volumeImpact * slippageMultiplier;
        var adjustedExitPrice = exitPrice * (1 - slippage);

        var grossExitValue = adjustedExitPrice * exitVolume;
        var taxAmount = grossExitValue * taxRate;
        var taxCap = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var cappedTax = Math.Min(taxAmount, taxCap);

        var netProceeds = grossExitValue - cappedTax;
        var costBasis = pos.EntryPrice * exitVolume;
        var netPnl = netProceeds - costBasis;

        return (netProceeds, exitVolume, netPnl);
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

    /// <summary>
    ///     Builds PriceBuckets from pre-aggregated daily trade data,
    ///     used by MeanReversionExecutor and similar executors that need
    ///     price distribution in their scoring context.
    /// </summary>
    internal static IReadOnlyList<PriceBucket> BuildPriceBucketsFromAggregates(
        List<DailyTradeAggregate> aggregates
    )
    {
        if (aggregates.Count == 0)
        {
            return [];
        }

        var bucketSize = Math.Max(1, aggregates.Count / PriceBucketCount);
        var buckets = new List<PriceBucket>();

        for (var i = 0; i < aggregates.Count; i += bucketSize)
        {
            var remaining = Math.Min(bucketSize, aggregates.Count - i);
            var chunk = aggregates.GetRange(i, remaining);

            var totalVolume = chunk.Sum(a => a.TotalVolume);
            var weightedAvgPrice = totalVolume > 0
                ? chunk.Sum(a => a.AveragePrice * a.TotalVolume) / totalVolume
                : chunk.Average(a => a.AveragePrice);

            buckets.Add(
                new PriceBucket(
                    chunk[0].Date.ToDateTime(TimeOnly.MinValue),
                    weightedAvgPrice,
                    chunk.Min(a => a.MinPrice),
                    chunk.Max(a => a.MaxPrice),
                    totalVolume
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