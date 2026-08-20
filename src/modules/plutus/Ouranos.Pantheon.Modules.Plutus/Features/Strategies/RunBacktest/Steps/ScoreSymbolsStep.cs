using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class ScoreSymbolsStep(
    ILogger<ScoreSymbolsStep> logger,
    ISignalScoringService signalScoringService
) : IStep<BacktestPayload>
{
    private readonly ILogger<ScoreSymbolsStep> _logger = Guard.Against.Null(logger);
    private readonly ISignalScoringService _signalScoringService = Guard.Against.Null(
        signalScoringService
    );

    /// <summary>
    ///     Number of prior days' reconstructed signal values to retain per (symbol,
    ///     signal type) for the 70/30 latest/trend blend in
    ///     <see cref="Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting.Scorers.SignalInputScorerBase" />. The live recommendations path loads the same
    ///     number of daily values from <c>signal_history_30m</c> via a
    ///     <c>time_bucket('1 day', bucket)</c> aggregation (see
    ///     <c>GetRecommendationsHandler.LoadSignalHistoryAsync</c>), so the optimization
    ///     fitness trains against the same daily-trend smoothing the live scorer
    ///     applies.
    /// </summary>
    internal const int SignalHistoryWindowSize = 6;

    public async Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        Guard.Against.Null(payload.Context);

        var ctx = payload.Context;
        var currentDate = ctx.CurrentDate(context);

        var scored = new List<ScoredSymbol>();

        foreach (var symbol in ctx.Data.Symbols)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var aggregates = ctx.Data.GetWindowAggregates(
                symbol.Id,
                DateTimeOffset.MinValue,
                currentDate,
                windowDays: 30
            );
            if (aggregates.Count == 0)
            {
                continue;
            }

            var currentPrice = aggregates.MaxBy(a => a.Date)?.AveragePrice ?? 0;
            if (currentPrice == 0)
            {
                continue;
            }

            var snapshots = ctx.Data.GetSnapshotsForSymbol(symbol.Id, currentDate);
            var priceBuckets = BacktestMath.BuildPriceBucketsFromAggregates(aggregates);

            var limit = ctx.Data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue;

            var signalHistory = GetSignalHistory(payload, symbol.Id);

            var scoreResult = await _signalScoringService.BuildScoreContextAsync(
                symbol.Id,
                payload.Parameters.MarketId,
                symbol.Name,
                symbol.Subcode,
                currentPrice,
                ctx.TaxRate,
                limit,
                snapshots,
                priceBuckets,
                ctx.InputWeights,
                ctx.Thresholds,
                signalHistory,
                context.CancellationToken
            );

            var score = ctx.Executor.Score(scoreResult.Context, payload.Parameters.Configuration);
            if (score.HasValue)
            {
                scored.Add(new ScoredSymbol(symbol, score.Value, currentPrice));
            }

            AppendSignalHistory(payload, symbol.Id, scoreResult.Signals);
        }

        _logger.LogDebug(
            "Day {currentDate}: scored {scoredCount}/{symbolCount} symbols.",
            currentDate,
            scored.Count,
            ctx.Data.Symbols.Count
        );

        payload.Portfolio.ScoredSymbols = scored;
    }

    /// <summary>
    ///     Returns the rolling signal history for a symbol as the read-only shape
    ///     <see cref="StrategyScoreContext" /> expects, or <c>null</c> when no history
    ///     has been accumulated yet (the scorer falls back to latest-value-only).
    /// </summary>
    internal static IReadOnlyDictionary<SignalType, IReadOnlyList<decimal>>? GetSignalHistory(
        BacktestPayload payload,
        Id<Symbol> symbolId
    )
    {
        if (payload.SignalHistoryBuffer.TryGetValue(symbolId, out var byType) && byType.Count > 0)
        {
            return byType.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<decimal>)[.. kv.Value]);
        }

        return null;
    }

    /// <summary>
    ///     Pushes the current day's reconstructed signal values into the rolling
    ///     buffer, capping each (symbol, signal type) series at
    ///     <see cref="SignalHistoryWindowSize" /> entries (oldest dropped first).
    /// </summary>
    internal static void AppendSignalHistory(
        BacktestPayload payload,
        Id<Symbol> symbolId,
        IReadOnlyList<Signal> signals
    )
    {
        if (signals.Count == 0)
        {
            return;
        }

        if (!payload.SignalHistoryBuffer.TryGetValue(symbolId, out var byType))
        {
            byType = [];
            payload.SignalHistoryBuffer[symbolId] = byType;
        }

        foreach (var signal in signals)
        {
            if (!byType.TryGetValue(signal.Type, out var series))
            {
                series = [];
                byType[signal.Type] = series;
            }

            series.Add(signal.Value);
            if (series.Count > SignalHistoryWindowSize)
            {
                series.RemoveAt(0);
            }
        }
    }
}
