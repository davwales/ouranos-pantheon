using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class BuyCandidatesStep(ILogger<BuyCandidatesStep> logger) : IStep<BacktestPayload>
{
    private readonly ILogger<BuyCandidatesStep> _logger = Guard.Against.Null(logger);

    /// <summary>Below this daily volume, skip the participation cap to avoid zeroing out strong-signal candidates on thin days.</summary>
    private const decimal LowVolumeCapThreshold = 10m;

    public Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (payload.Context is not { } ctx)
        {
            throw new InvalidOperationException("Backtest context is not initialized.");
        }

        var currentDate = ctx.CurrentDate(context);
        var configuration = payload.Parameters.Configuration;
        var buyThreshold = payload.Parameters.Thresholds.BuyThreshold ?? 0m;
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
            TryBuyCandidate(candidate, payload, ctx, currentDate, maxPositionPercent);
        }

        return Task.CompletedTask;
    }

    private void TryBuyCandidate(
        ScoredSymbol candidate,
        BacktestPayload payload,
        BacktestContext ctx,
        DateTimeOffset currentDate,
        decimal maxPositionPercent
    )
    {
        var maxPositionBudget = payload.Portfolio.Balance * maxPositionPercent;

        var dailyVolume = ctx.Data.GetDailyVolume(candidate.Symbol.Id, currentDate);
        var volumeImpact =
            dailyVolume > 0 ? Math.Max(1m, maxPositionBudget / candidate.Price) / dailyVolume : 0m;
        var slippage = volumeImpact * payload.Parameters.SlippageMultiplier;
        var adjustedBuyPrice = candidate.Price * (1 + slippage);

        var volume = Math.Floor(maxPositionBudget / adjustedBuyPrice);
        if (volume < 1 || adjustedBuyPrice == 0)
        {
            return;
        }

        volume = ClampToSymbolLimit(candidate, volume);
        if (volume == 0)
        {
            return;
        }

        volume = ClampToParticipationCap(
            volume,
            dailyVolume,
            payload.Parameters.VolumeParticipationRate
        );
        if (volume < 1)
        {
            return;
        }

        var cost = adjustedBuyPrice * volume;
        if (cost > payload.Portfolio.Balance)
        {
            return;
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

    private decimal ClampToSymbolLimit(ScoredSymbol candidate, decimal volume)
    {
        var symbolLimit = candidate.Symbol.AdditionalFields.Limit;
        if (!symbolLimit.HasValue || volume <= symbolLimit.Value)
        {
            return volume;
        }

        if (symbolLimit.Value == 0)
        {
            _logger.LogWarning(
                "Skipping strong-signal candidate {SymbolName} (score {Score:F3}) because its per-symbol trade limit is 0.",
                candidate.Symbol.Name,
                candidate.Score
            );
            return 0;
        }

        return symbolLimit.Value;
    }

    private static decimal ClampToParticipationCap(
        decimal volume,
        decimal dailyVolume,
        decimal participationRate
    )
    {
        if (dailyVolume <= 0 || dailyVolume < LowVolumeCapThreshold)
        {
            return volume;
        }

        var maxBuyableVolume = Math.Floor(dailyVolume * participationRate);
        return Math.Min(volume, maxBuyableVolume);
    }
}
