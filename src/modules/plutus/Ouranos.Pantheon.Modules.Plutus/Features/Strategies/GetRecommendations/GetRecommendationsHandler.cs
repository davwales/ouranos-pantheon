using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Extensions;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Querying;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandler
    : IPantheonHandler<GetRecommendationsInput, GetRecommendationsResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetRecommendationsHandler> _logger;
    private readonly IStrategyExecutor _executor;

    public GetRecommendationsHandler(
        ILogger<GetRecommendationsHandler> logger,
        PlutusDbContext dbContext,
        IStrategyExecutor executor
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(executor);

        _logger = logger;
        _dbContext = dbContext;
        _executor = executor;
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

        var recommendations = new List<StrategyRecommendation>();
        var buildContext = new RecommendationBuildContext(
            data.Snapshots,
            data.Signals,
            data.SignalHistory,
            data.Strategy,
            query.MarketId,
            taxRate,
            limit,
            buyThreshold,
            query.Budget,
            maxPositionPercent,
            _executor
        );

        foreach (var symbol in data.Symbols)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recommendation = TryBuildRecommendation(buildContext, symbol);
            if (recommendation is not null)
            {
                recommendations.Add(recommendation);
            }
        }

        var sorted = recommendations.OrderByDescending(r => r.Score).Take(maxPositions).ToList();

        _logger.LogDebug(
            "Successfully handled get recommendations request. {count} recommendations.",
            sorted.Count
        );
        return new GetRecommendationsResponse(sorted);
    }

    private static StrategyRecommendation? TryBuildRecommendation(
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

        var positionBudget = buildContext.Budget * buildContext.MaxPositionPercent;
        var volume = Math.Max(1, Math.Floor(positionBudget / currentPrice));
        var allocation = volume * currentPrice;

        return new StrategyRecommendation(
            symbol.Id.ToString(),
            symbol.Name,
            symbol.Subcode,
            score.Value,
            allocation,
            currentPrice,
            volume,
            BuildRationale(score.Value, symbolSignals, symbolMedium)
        );
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

        return new RecommendationData(strategy, market, symbols, snapshots, signals, signalHistory);
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
