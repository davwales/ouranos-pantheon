using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class BuyCandidatesStep : IStep<BacktestPayload>
{
    public Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        Guard.Against.Null(payload.Context);

        var currentDate = payload.Context.CurrentDate(context);
        var configuration = payload.Parameters.Configuration;
        var buyThreshold = payload.Parameters.SignalWeightedConfig?.BuyThreshold ?? 0m;
        var maxPositions = configuration.MaxPositions ?? int.MaxValue;
        var maxPositionPercent = configuration.MaxPositionPercent ?? 1m;

        var scoredSymbols = payload.Portfolio.ScoredSymbols ?? [];

        var candidates = scoredSymbols
            .Where(s =>
                s.Score > buyThreshold && !payload.Portfolio.OpenPositions.ContainsKey(s.Symbol.Id)
            )
            .OrderByDescending(s => s.Score)
            .Take(Math.Max(0, maxPositions - payload.Portfolio.OpenPositions.Count))
            .ToList();

        foreach (var candidate in candidates)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var ctx = payload.Context;
            var maxPositionBudget = payload.Portfolio.Balance * maxPositionPercent;

            var dailyVolume = ctx.Data.GetDailyVolume(candidate.Symbol.Id, currentDate);
            var volumeImpact =
                dailyVolume > 0
                    ? Math.Max(1m, maxPositionBudget / candidate.Price) / dailyVolume
                    : 0m;
            var slippage = volumeImpact * payload.Parameters.SlippageMultiplier;
            var adjustedBuyPrice = candidate.Price * (1 + slippage);

            var buyingPower = maxPositionBudget / adjustedBuyPrice;
            var volume = Math.Floor(buyingPower);

            if (volume < 1 || adjustedBuyPrice == 0)
            {
                continue;
            }

            var symbolLimit = candidate.Symbol.AdditionalFields.Limit;
            if (volume > symbolLimit)
            {
                volume = symbolLimit.Value;
            }

            if (dailyVolume > 0)
            {
                var maxBuyableVolume = Math.Floor(
                    dailyVolume * payload.Parameters.VolumeParticipationRate
                );
                volume = Math.Min(volume, maxBuyableVolume);
            }

            if (volume < 1)
            {
                continue;
            }

            var cost = adjustedBuyPrice * volume;
            if (cost > payload.Portfolio.Balance)
            {
                continue;
            }

            payload.Portfolio.Balance -= cost;
            payload.Portfolio.OpenPositions[candidate.Symbol.Id] = new OpenPosition(
                candidate.Symbol.Id,
                candidate.Symbol.Name,
                candidate.Symbol.Subcode,
                adjustedBuyPrice,
                volume,
                currentDate
            );
        }

        return Task.CompletedTask;
    }
}
