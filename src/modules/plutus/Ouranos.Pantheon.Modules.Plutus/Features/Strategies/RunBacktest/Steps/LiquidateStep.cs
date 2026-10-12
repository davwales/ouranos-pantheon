using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class LiquidateStep : IStep<BacktestPayload>
{
    public Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        Guard.Against.Null(payload.Context);

        var ctx = payload.Context;
        var endDate = payload.Parameters.EndDate;

        foreach (var open in payload.Portfolio.OpenPositions.Values.ToList())
        {
            var pos = open;
            var day = ctx.Data.GetDailyAggregate(pos.SymbolId, endDate);

            if (day is not null && day.AveragePrice > 0)
            {
                var fillCap = ctx.Data.GetFillCap(
                    pos.SymbolId,
                    endDate,
                    payload.Parameters.VolumeParticipationRate
                );

                var (netProceeds, exitVolume, netPnl) = BacktestMath.ComputeExit(
                    pos,
                    day.AveragePrice,
                    ctx.TaxRate,
                    ctx.Data.Market,
                    day.TotalVolume,
                    fillCap,
                    payload.Parameters.SlippageMultiplier
                );

                if (exitVolume > 0)
                {
                    payload.Portfolio.Balance += netProceeds;
                    var (remaining, closed) = BacktestMath.ApplyExit(
                        pos,
                        day.AveragePrice,
                        exitVolume,
                        netPnl,
                        endDate
                    );

                    if (closed is not null)
                    {
                        payload.Portfolio.ClosedPositions.Add(closed);
                        continue;
                    }

                    pos = remaining ?? pos;
                }
            }

            MarkResidualToMarket(pos, payload, ctx, endDate);
        }

        payload.Portfolio.OpenPositions.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Volume the liquidity cap could not sell by the end of the run is valued at the latest
    ///     day's minimum traded price (net of tax) rather than the average, so stranded inventory
    ///     in thin items cannot inflate results.
    /// </summary>
    private static void MarkResidualToMarket(
        OpenPosition pos,
        BacktestPayload payload,
        BacktestContext ctx,
        DateTimeOffset endDate
    )
    {
        var latest = ctx.Data.GetLatestAggregate(pos.SymbolId, endDate);
        var markPrice = latest is { MinPrice: > 0 } ? latest.MinPrice : pos.EntryPrice;

        var grossValue = markPrice * pos.Volume;

        var netProceeds =
            grossValue - BacktestMath.ComputeTax(grossValue, ctx.TaxRate, ctx.Data.Market);

        var netPnl = netProceeds - (pos.EntryPrice * pos.Volume);

        payload.Portfolio.Balance += netProceeds;

        var (_, closed) = BacktestMath.ApplyExit(pos, markPrice, pos.Volume, netPnl, endDate);
        if (closed is not null)
        {
            payload.Portfolio.ClosedPositions.Add(closed);
        }
    }
}
