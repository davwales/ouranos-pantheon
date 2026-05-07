using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class ScoreSymbolsStep(
    ILogger<ScoreSymbolsStep> logger,
    IEnumerable<ISignalComputer> signalComputers
) : IStep<BacktestPayload>
{
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

            var aggregates = ctx.Data.GetWindowAggregates(symbol.Id, DateTimeOffset.MinValue, currentDate);
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
            var forecast = ctx.Data.GetForecastForSymbol(symbol.Id, currentDate);
            var (forecastedPrice, forecastedChange) = BacktestMath.GetForecastData(forecast, currentPrice);

            var limit = ctx.Data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue;

            var signals = await ReconstructSignalsAsync(
                symbol.Id,
                ctx.TaxRate,
                snapshots,
                priceBuckets,
                limit,
                context.CancellationToken
            );

            var scoreContext = new StrategyScoreContext(
                symbol.Id,
                payload.Parameters.MarketId,
                symbol.Name,
                symbol.Subcode,
                currentPrice,
                ctx.TaxRate,
                ctx.Data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue,
                snapshots.Short,
                snapshots.Medium,
                snapshots.Long,
                priceBuckets,
                signals,
                forecastedPrice,
                forecastedChange
            );

            var score = ctx.Executor.Score(scoreContext, payload.Parameters.Configuration);
            if (score.HasValue)
            {
                scored.Add(new ScoredSymbol(symbol, score.Value, currentPrice));
            }
        }

        logger.LogDebug(
            "Day {currentDate}: scored {scoredCount}/{symbolCount} symbols.",
            currentDate,
            scored.Count,
            ctx.Data.Symbols.Count
        );

        payload.Portfolio.ScoredSymbols = scored;
    }

    private async Task<IReadOnlyList<Signal>> ReconstructSignalsAsync(
        Id<Symbol> symbolId,
        decimal taxRate,
        (MarketTradeSnapshot? Short, MarketTradeSnapshot? Medium, MarketTradeSnapshot? Long) snapshots,
        IReadOnlyList<PriceBucket> priceBuckets,
        decimal limit,
        CancellationToken cancellationToken
    )
    {
        var signals = new List<Signal>();

        foreach (var computer in signalComputers)
        {
            var computeContext = new SignalComputeContext(
                symbolId,
                snapshots.Short?.MarketId ?? default,
                taxRate,
                limit,
                snapshots.Short,
                snapshots.Medium,
                snapshots.Long,
                priceBuckets
            );

            var value = await computer.ComputeAsync(computeContext, cancellationToken);
            if (value.HasValue)
            {
                signals.Add(Signal.Create(default, symbolId, computer.Type, value.Value));
            }
        }

        return signals;
    }
}
