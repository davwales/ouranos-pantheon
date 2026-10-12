using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Sizing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandler
    : IPantheonHandler<GetRecommendationsInput, GetRecommendationsResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetRecommendationsHandler> _logger;
    private readonly IStrategyExecutor _executor;
    private readonly IOptions<BacktestDataOptions> _executionOptions;

    public GetRecommendationsHandler(
        ILogger<GetRecommendationsHandler> logger,
        PlutusDbContext dbContext,
        IStrategyExecutor executor,
        IOptions<BacktestDataOptions> executionOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(executor);
        Guard.Against.Null(executionOptions);

        _logger = logger;
        _dbContext = dbContext;
        _executor = executor;
        _executionOptions = executionOptions;
    }

    public async Task<GetRecommendationsResponse> Handle(
        GetRecommendationsInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recommendations query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.NegativeOrZero(query.Budget, nameof(query.Budget));

        var data = await LoadRecommendationDataAsync(query, cancellationToken);

        if (data.Symbols.Count == 0)
        {
            return new GetRecommendationsResponse([]);
        }

        var taxRate = data.Market.Taxes.Flat?.Rate ?? 0m;
        var limit = data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var buyThreshold = data.Strategy.Thresholds.BuyThreshold ?? 0m;
        var maxPositions = data.Strategy.TradingConfiguration.MaxPositions ?? int.MaxValue;
        var maxPositionPercent = data.Strategy.TradingConfiguration.MaxPositionPercent ?? 1m;

        var buildContext = new RecommendationBuildContext(
            data.Snapshots,
            data.Signals,
            data.SignalHistory,
            data.Strategy,
            query.MarketId,
            taxRate,
            limit,
            buyThreshold,
            _executor
        );

        var candidates = new List<RecommendationCandidate>();
        foreach (var symbol in data.Symbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var candidate = TryScoreCandidate(buildContext, symbol);
            if (candidate is not null)
            {
                candidates.Add(candidate);
            }
        }

        var sorted = AllocateBudget(
            candidates.OrderByDescending(c => c.Score),
            data.TypicalDailyVolumes,
            query.Budget,
            maxPositions,
            maxPositionPercent
        );

        _logger.LogDebug(
            "Successfully handled get recommendations request. {count} recommendations.",
            sorted.Count
        );
        return new GetRecommendationsResponse(sorted);
    }

    private static RecommendationCandidate? TryScoreCandidate(
        RecommendationBuildContext buildContext,
        Symbol symbol
    )
    {
        var symbolShort = buildContext.Snapshots.FirstOrDefault(s =>
            s.SymbolId == symbol.Id && s.TimeFrame == TimeFrame.OneHour
        );
        var symbolMedium = buildContext.Snapshots.FirstOrDefault(s =>
            s.SymbolId == symbol.Id && s.TimeFrame == TimeFrame.OneWeek
        );
        var symbolLong = buildContext.Snapshots.FirstOrDefault(s =>
            s.SymbolId == symbol.Id && s.TimeFrame == TimeFrame.OneMonth
        );

        var priceSnapshot = FindPriceSnapshot(buildContext.Snapshots, symbol.Id);
        if (priceSnapshot is null)
        {
            return null;
        }

        var currentPrice = priceSnapshot.TotalSpent / priceSnapshot.TotalVolume;

        var symbolSignals = buildContext.Signals.Where(s => s.SymbolId == symbol.Id).ToList();
        var symbolHistory = buildContext.SignalHistory.GetValueOrDefault(symbol.Id);

        var context = new StrategyScoreContext(
            symbol.Id,
            buildContext.MarketId,
            symbol.Name,
            symbol.Subcode,
            currentPrice,
            buildContext.TaxRate,
            buildContext.Limit,
            priceSnapshot,
            symbolMedium,
            symbolLong,
            [],
            symbolSignals,
            buildContext.Strategy.InputWeights,
            buildContext.Strategy.Thresholds,
            symbolHistory
        );

        var score = buildContext.Executor.Score(
            context,
            buildContext.Strategy.TradingConfiguration
        );
        if (score is null || score.Value <= buildContext.BuyThreshold)
        {
            return null;
        }

        return new RecommendationCandidate(
            symbol,
            score.Value,
            currentPrice,
            BuildRationale(score.Value, symbolSignals, symbolMedium)
        );
    }

    /// <summary>
    ///     Walks candidates best-first, sizing each from the budget still unallocated with the
    ///     same <see cref="PositionSizer" /> the backtest uses, so suggested volumes respect the
    ///     typical daily volume and per-symbol buy limits and never sum past the budget.
    ///     Candidates that cannot be meaningfully filled are skipped rather than taking a slot.
    /// </summary>
    private List<StrategyRecommendation> AllocateBudget(
        IEnumerable<RecommendationCandidate> candidates,
        Dictionary<Id<Symbol>, decimal> typicalDailyVolumes,
        decimal budget,
        int maxPositions,
        decimal maxPositionPercent
    )
    {
        var options = _executionOptions.Value;
        var remaining = budget;
        var recommendations = new List<StrategyRecommendation>();

        foreach (var candidate in candidates)
        {
            if (recommendations.Count >= maxPositions || remaining <= 0)
            {
                break;
            }

            var typicalVolume = typicalDailyVolumes.GetValueOrDefault(candidate.Symbol.Id);
            var size = PositionSizer.Size(
                candidate.Price,
                Math.Min(budget * maxPositionPercent, remaining),
                Math.Floor(typicalVolume * BacktestDataOptions.DefaultVolumeParticipationRate),
                candidate.Symbol.AdditionalFields.Limit,
                options.LimitWindowsPerDay,
                options.MinFillFraction
            );

            if (size.Volume < 1)
            {
                continue;
            }

            var allocation = size.Volume * candidate.Price;
            remaining -= allocation;

            recommendations.Add(
                new StrategyRecommendation(
                    candidate.Symbol.Id.ToString(),
                    candidate.Symbol.Name,
                    candidate.Symbol.Subcode,
                    candidate.Score,
                    allocation,
                    candidate.Price,
                    size.Volume,
                    typicalVolume,
                    size.LimitedBy,
                    candidate.Rationale
                )
            );
        }

        return recommendations;
    }

    private static MarketTradeSnapshot? FindPriceSnapshot(
        List<MarketTradeSnapshot> snapshots,
        Id<Symbol> symbolId
    )
    {
        TimeFrame[] priceTimeframes =
        [
            TimeFrame.OneHour,
            TimeFrame.OneDay,
            TimeFrame.OneWeek,
            TimeFrame.OneMonth,
        ];

        foreach (var timeframe in priceTimeframes)
        {
            var snapshot = snapshots.FirstOrDefault(s =>
                s.SymbolId == symbolId && s.TimeFrame == timeframe
            );
            if (snapshot is { TotalSpent: > 0, TotalVolume: > 0 })
            {
                return snapshot;
            }
        }

        return null;
    }

    private static string BuildRationale(
        decimal score,
        List<Signal> signals,
        MarketTradeSnapshot? snap
    )
    {
        var parts = new List<string> { $"Score: {score:F3}" };

        if (signals.Count > 0)
        {
            var topSignals = signals.OrderByDescending(s => Math.Abs(s.Value)).Take(3);
            parts.Add(
                $"Top signals: {string.Join(", ", topSignals.Select(s => $"{s.Type}={s.Value:F2}"))}"
            );
        }

        if (snap is not null)
        {
            parts.Add($"Price range: {snap.MinPrice:F2}-{snap.MaxPrice:F2}");
        }

        return string.Join("; ", parts);
    }

    /// <summary>
    ///     Loads every input the recommendation loop needs - strategy, market, symbols,
    ///     trade snapshots, and latest signals (plus reconstructed history) - in one
    ///     place so <see cref="Handle" /> is a plain validate -&gt; load -&gt; loop -&gt; sort
    ///     flow. Read-only path: all queries use <c>AsNoTracking</c>.
    /// </summary>
    private async Task<RecommendationData> LoadRecommendationDataAsync(
        GetRecommendationsInput query,
        CancellationToken cancellationToken
    )
    {
        var strategy = await _dbContext
            .Strategies.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.StrategyId, cancellationToken);
        Guard.Against.NotFound(query.StrategyId, strategy);

        var market = await _dbContext
            .Markets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.MarketId, cancellationToken);
        Guard.Against.NotFound(query.MarketId, market);

        Guard.Against.InvalidInput(
            query.MarketId,
            nameof(query.MarketId),
            m => m == strategy.MarketId,
            $"Strategy '{strategy.Id}' does not belong to market '{query.MarketId}'."
        );

        var symbols = await _dbContext
            .Symbols.AsNoTracking()
            .Where(s => s.MarketId == query.MarketId)
            .ToListAsync(cancellationToken);

        var symbolIds = symbols.Select(s => s.Id).ToList();

        var snapshots = await _dbContext
            .MarketTradeSnapshots.AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId) && s.MarketId == query.MarketId)
            .ToListAsync(cancellationToken);

        var latestRows = await _dbContext
            .LatestSignals.AsNoTracking()
            .Where(s => symbolIds.Contains(s.SymbolId))
            .ToListAsync(cancellationToken);

        var signals = latestRows
            .Select(ls => Signal.Create(query.MarketId, ls.SymbolId, ls.SignalType, ls.LastValue))
            .ToList();

        var signalHistory = await TryLoadSignalHistoryAsync(symbolIds, cancellationToken);
        var typicalDailyVolumes = await TryLoadTypicalDailyVolumesAsync(
            symbolIds,
            snapshots,
            cancellationToken
        );

        return new RecommendationData(
            strategy,
            market,
            symbols,
            snapshots,
            signals,
            signalHistory,
            typicalDailyVolumes
        );
    }

    private async Task<Dictionary<Id<Symbol>, decimal>> TryLoadTypicalDailyVolumesAsync(
        List<Id<Symbol>> symbolIds,
        List<MarketTradeSnapshot> snapshots,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await LoadTypicalDailyVolumesAsync(symbolIds, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(
                ex,
                "Daily volume query failed; falling back to the weekly snapshot average."
            );

            return snapshots
                .Where(s => s.TimeFrame == TimeFrame.OneWeek)
                .GroupBy(s => s.SymbolId)
                .ToDictionary(g => g.Key, g => g.First().TotalVolume / 7m);
        }
    }

    /// <summary>
    ///     Median daily volume over the last <see cref="LiquidityMath.TypicalVolumeDays" />
    ///     complete days per symbol, matching the liquidity cap the backtest applies.
    /// </summary>
    private async Task<Dictionary<Id<Symbol>, decimal>> LoadTypicalDailyVolumesAsync(
        List<Id<Symbol>> symbolIds,
        CancellationToken cancellationToken
    )
    {
        if (symbolIds.Count == 0)
        {
            return [];
        }

        var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var command = RawSqlCommand
            .FromSql(
                """
                SELECT symbol_id, volume
                FROM plutus.trades_daily
                WHERE symbol_id = ANY(@symbolIds)
                  AND bucket >= @from
                  AND bucket < @to
                """
            )
            .WithIds("@symbolIds", symbolIds)
            .WithDateTimeOffset("@from", today.AddDays(-LiquidityMath.TypicalVolumeDays))
            .WithDateTimeOffset("@to", today);

        var rows = await _dbContext.Database.ExecuteQueryAsync<DailyVolumeRow>(
            command,
            cancellationToken
        );

        return rows.GroupBy(r => new Id<Symbol>(r.SymbolId.ToString()))
            .ToDictionary(
                g => g.Key,
                g =>
                    LiquidityMath.MedianDailyVolume(
                        g.Select(r => r.Volume),
                        LiquidityMath.TypicalVolumeDays
                    )
            );
    }

    private async Task<
        Dictionary<Id<Symbol>, Dictionary<SignalType, IReadOnlyList<decimal>>>
    > TryLoadSignalHistoryAsync(List<Id<Symbol>> symbolIds, CancellationToken cancellationToken)
    {
        try
        {
            return await LoadSignalHistoryAsync(symbolIds, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(
                ex,
                "Signal history query failed; falling back to latest-value-only scoring."
            );
            return [];
        }
    }

    /// <summary>
    ///     Loads the most recent <see cref="ScoreSymbolsStep.SignalHistoryWindowSize" />
    ///     daily signal values per (symbol, signal type) from the
    ///     <c>signal_history_30m</c> continuous aggregate, collapsing its 30-minute
    ///     buckets into one value per day via <c>time_bucket('1 day', bucket)</c>.
    ///     This matches the backtest path (<see cref="ScoreSymbolsStep" />), which
    ///     reconstructs one signal value per day and keeps a rolling buffer of the
    ///     same size, so the 70/30 latest/trend blend applied live is the same blend
    ///     the optimizer trained against.
    /// </summary>
    private async Task<
        Dictionary<Id<Symbol>, Dictionary<SignalType, IReadOnlyList<decimal>>>
    > LoadSignalHistoryAsync(List<Id<Symbol>> symbolIds, CancellationToken cancellationToken)
    {
        if (symbolIds.Count == 0)
        {
            return [];
        }

        var command = RawSqlCommand
            .FromSql(
                """
                SELECT
                    symbol_id,
                    signal_type,
                    time_bucket('1 day', bucket) AS bucket,
                    LAST(last_value, bucket) AS last_value
                FROM plutus.signal_history_30m
                WHERE symbol_id = ANY(@symbolIds)
                  AND bucket >= now() - INTERVAL '7 days'
                GROUP BY symbol_id, signal_type, time_bucket('1 day', bucket)
                ORDER BY symbol_id, signal_type, bucket
                """
            )
            .WithIds("@symbolIds", symbolIds);

        var rows = await _dbContext.Database.ExecuteQueryAsync<SignalHistoryRow>(
            command,
            cancellationToken
        );

        return rows.GroupBy(r => new Id<Symbol>(r.SymbolId.ToString()))
            .ToDictionary(
                g => g.Key,
                g =>
                    g.GroupBy(r => (SignalType)r.SignalType)
                        .ToDictionary(
                            sg => sg.Key,
                            sg =>
                                (IReadOnlyList<decimal>)
                                    [
                                        .. sg.TakeLast(ScoreSymbolsStep.SignalHistoryWindowSize)
                                            .Select(r => r.LastValue ?? 0m),
                                    ]
                        )
            );
    }
}
