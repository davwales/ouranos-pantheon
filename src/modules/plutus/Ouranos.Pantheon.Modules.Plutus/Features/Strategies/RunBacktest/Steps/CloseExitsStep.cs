using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class CloseExitsStep(IEnumerable<ISignalComputer> signalComputers) : IStep<BacktestPayload>
{
    public async Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        Guard.Against.Null(payload.Context);

        var ctx = payload.Context;
        var holdLimit = payload.Parameters.Configuration.HoldPeriodDays ?? int.MaxValue;
        var sellThreshold = payload.Parameters.Configuration.SellThreshold;

        var toClose = new List<KeyValuePair<Id<Symbol>, OpenPosition>>();

        var currentDate = ctx.CurrentDate(context);

        foreach (var kvp in payload.Portfolio.OpenPositions)
        {
            if ((currentDate - kvp.Value.EntryTime).Days >= holdLimit)
            {
                toClose.Add(kvp);
            }
        }

        if (sellThreshold.HasValue)
        {
            foreach (var kvp in payload.Portfolio.OpenPositions)
            {
                if (toClose.Any(p => p.Key.Equals(kvp.Key)))
                {
                    continue;
                }

                var shouldSell = await EvaluateSellSignalAsync(
                    kvp.Value.SymbolId,
                    kvp.Value.SymbolName,
                    kvp.Value.SymbolSubcode,
                    ctx,
                    payload,
                    context,
                    sellThreshold.Value
                );

                if (shouldSell)
                {
                    toClose.Add(kvp);
                }
            }
        }

        foreach (var kvp in toClose)
        {
            var exitPrice = ctx.Data.GetLatestPrice(kvp.Key, currentDate);

            if (exitPrice == 0)
            {
                continue;
            }

            var dailyVolume = ctx.Data.GetDailyVolume(kvp.Key, currentDate);
            var (netProceeds, exitVolume, netPnl) = BacktestMath.ComputeExit(
                kvp.Value,
                exitPrice,
                ctx.TaxRate,
                ctx.Data.Market,
                dailyVolume,
                payload.Parameters.VolumeParticipationRate,
                payload.Parameters.SlippageMultiplier
            );

            if (exitVolume <= 0)
            {
                continue;
            }

            payload.Portfolio.Balance += netProceeds;
            payload.Portfolio.ClosedPositions.Add(
                BacktestMath.CreateClosedPosition(kvp.Value, exitPrice, exitVolume, netPnl, currentDate)
            );

            if (exitVolume >= kvp.Value.Volume)
            {
                payload.Portfolio.OpenPositions.Remove(kvp.Key);
            }
            else
            {
                payload.Portfolio.OpenPositions[kvp.Key] = kvp.Value with { Volume = kvp.Value.Volume - exitVolume };
            }
        }
    }

    private async Task<bool> EvaluateSellSignalAsync(
        Id<Symbol> symbolId,
        string symbolName,
        string? symbolSubcode,
        BacktestContext ctx,
        BacktestPayload payload,
        PipelineContext context,
        decimal sellThreshold
    )
    {
        var currentDate = ctx.CurrentDate(context);
        var snapshots = ctx.Data.GetSnapshotsForSymbol(symbolId, currentDate);
        var currentPrice = ctx.Data.GetLatestPrice(symbolId, currentDate);

        if (currentPrice == 0)
        {
            return false;
        }

        var limit = ctx.Data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var forecast = ctx.Data.GetForecastForSymbol(symbolId, currentDate);
        var (forecastedPrice, forecastedChange) = BacktestMath.GetForecastData(forecast, currentPrice);

        var allAggregates = ctx.Data.GetWindowAggregates(symbolId, DateTimeOffset.MinValue, currentDate);
        var priceBuckets = BacktestMath.BuildPriceBucketsFromAggregates(allAggregates);

        var signals = await ReconstructSignalsAsync(
            symbolId,
            ctx.TaxRate,
            limit,
            snapshots,
            priceBuckets,
            context
        );

        var scoreContext = new StrategyScoreContext(
            symbolId,
            payload.Parameters.MarketId,
            symbolName,
            symbolSubcode,
            currentPrice,
            ctx.TaxRate,
            limit,
            snapshots.Short,
            snapshots.Medium,
            snapshots.Long,
            priceBuckets,
            signals,
            forecastedPrice,
            forecastedChange
        );

        var score = ctx.Executor.Score(scoreContext, payload.Parameters.Configuration);

        return score < sellThreshold;
    }

    private async Task<IReadOnlyList<Signal>> ReconstructSignalsAsync(
        Id<Symbol> symbolId,
        decimal taxRate,
        decimal limit,
        (MarketTradeSnapshot? Short, MarketTradeSnapshot? Medium, MarketTradeSnapshot? Long) snapshots,
        IReadOnlyList<PriceBucket> priceBuckets,
        PipelineContext context
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

            var value = await computer.ComputeAsync(computeContext, context.CancellationToken);
            if (value is not null)
            {
                signals.Add(Signal.Create(default, symbolId, computer.Type, value.Value));
            }
        }

        return signals;
    }
}
