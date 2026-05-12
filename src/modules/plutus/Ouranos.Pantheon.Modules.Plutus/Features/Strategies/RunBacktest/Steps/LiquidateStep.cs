using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class LiquidateStep : IStep<BacktestPayload>
{
    public Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        Guard.Against.Null(payload.Context);

        var ctx = payload.Context;
        var endDate = payload.Parameters.EndDate;

        foreach (var pos in payload.Portfolio.OpenPositions.Values.ToList())
        {
            var exitPrice = ctx.Data.GetLatestPrice(pos.SymbolId, endDate);
            if (exitPrice == 0)
            {
                exitPrice = pos.EntryPrice;
            }

            var dailyVolume = ctx.Data.GetDailyVolume(pos.SymbolId, endDate);
            var (netProceeds, exitVolume, netPnl) = BacktestMath.ComputeExit(
                pos,
                exitPrice,
                ctx.TaxRate,
                ctx.Data.Market,
                dailyVolume,
                payload.Parameters.VolumeParticipationRate,
                payload.Parameters.SlippageMultiplier
            );

            if (exitVolume <= 0)
            {
                var forcedExitPrice = pos.EntryPrice * 0.5m;
                var forcedGrossValue = forcedExitPrice * pos.Volume;
                var forcedTax = forcedGrossValue * ctx.TaxRate;
                var forcedTaxCap = ctx.Data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
                var forcedCappedTax = Math.Min(forcedTax, forcedTaxCap);
                var forcedNetProceeds = forcedGrossValue - forcedCappedTax;

                payload.Portfolio.Balance += forcedNetProceeds;
                payload.Portfolio.ClosedPositions.Add(
                    BacktestMath.CreateClosedPosition(
                        pos,
                        forcedExitPrice,
                        pos.Volume,
                        forcedNetProceeds - pos.EntryPrice * pos.Volume,
                        endDate
                    )
                );

                continue;
            }

            payload.Portfolio.Balance += netProceeds;
            payload.Portfolio.ClosedPositions.Add(
                BacktestMath.CreateClosedPosition(pos, exitPrice, exitVolume, netPnl, endDate)
            );

            if (exitVolume >= pos.Volume)
            {
                continue;
            }

            var remainingVolume = pos.Volume - exitVolume;
            var remainingCostBasis = pos.EntryPrice * remainingVolume;
            var forcedExitPrice2 = pos.EntryPrice * 0.5m;
            var forcedGrossValue2 = forcedExitPrice2 * remainingVolume;
            var forcedTax2 = forcedGrossValue2 * ctx.TaxRate;
            var forcedTaxCap2 = ctx.Data.Market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
            var forcedNetProceeds2 = forcedGrossValue2 - Math.Min(forcedTax2, forcedTaxCap2);

            payload.Portfolio.Balance += forcedNetProceeds2;
            payload.Portfolio.ClosedPositions.Add(
                BacktestMath.CreateClosedPosition(
                    pos with
                    {
                        Volume = remainingVolume,
                    },
                    forcedExitPrice2,
                    remainingVolume,
                    forcedNetProceeds2 - remainingCostBasis,
                    endDate
                )
            );
        }

        payload.Portfolio.OpenPositions.Clear();
        return Task.CompletedTask;
    }
}
