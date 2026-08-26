using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
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
                var (forcedProceeds, forcedClosed) = ForceLiquidate(
                    pos,
                    exitPrice,
                    endDate,
                    ctx.TaxRate,
                    ctx.Data.Market
                );
                payload.Portfolio.Balance += forcedProceeds;
                payload.Portfolio.ClosedPositions.Add(forcedClosed);
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
            var (residualProceeds, residualClosed) = ForceLiquidate(
                pos with
                {
                    Volume = remainingVolume,
                },
                exitPrice,
                endDate,
                ctx.TaxRate,
                ctx.Data.Market
            );
            payload.Portfolio.Balance += residualProceeds;
            payload.Portfolio.ClosedPositions.Add(residualClosed);
        }

        payload.Portfolio.OpenPositions.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Mark-to-market liquidation for the volume a participation cap could not
    ///     sell. Computes gross value at <paramref name="exitPrice" />, applies the
    ///     flat tax capped at the market maximum, and produces the closed-position
    ///     record with the resulting net P&amp;L. Used both when no volume could be
    ///     sold at all and when only a partial fill left residue behind.
    /// </summary>
    private static (decimal NetProceeds, BacktestPosition ClosedPosition) ForceLiquidate(
        OpenPosition position,
        decimal exitPrice,
        DateTimeOffset endDate,
        decimal taxRate,
        Market market
    )
    {
        var grossValue = exitPrice * position.Volume;
        var tax = grossValue * taxRate;
        var taxCap = market.Taxes.Flat?.Maximum ?? decimal.MaxValue;
        var netProceeds = grossValue - Math.Min(tax, taxCap);
        var costBasis = position.EntryPrice * position.Volume;

        var closedPosition = BacktestMath.CreateClosedPosition(
            position,
            exitPrice,
            position.Volume,
            netProceeds - costBasis,
            endDate
        );

        return (netProceeds, closedPosition);
    }
}
