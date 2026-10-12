using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class BuyCandidatesStep(ILogger<BuyCandidatesStep> logger) : IStep<BacktestPayload>
{
    private readonly ILogger<BuyCandidatesStep> _logger = Guard.Against.Null(logger);

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
        var day = ctx.Data.GetDailyAggregate(candidate.Symbol.Id, currentDate);
        if (day is null || day.AveragePrice <= 0)
        {
            return;
        }

        var fillCap = ClampToSymbolLimit(
            candidate,
            ctx.Data.GetFillCap(
                candidate.Symbol.Id,
                currentDate,
                payload.Parameters.VolumeParticipationRate
            ),
            payload.Parameters.LimitWindowsPerDay
        );

        var maxPositionBudget = Math.Min(
            payload.Portfolio.Balance * maxPositionPercent,
            payload.Portfolio.Balance
        );

        var volume = Math.Min(Math.Floor(maxPositionBudget / day.AveragePrice), fillCap);
        if (volume < 1)
        {
            return;
        }

        var slippage = BacktestMath.ComputeSlippage(
            volume,
            day.TotalVolume,
            payload.Parameters.SlippageMultiplier
        );

        var adjustedBuyPrice = day.AveragePrice * (1 + slippage);
        volume = Math.Min(volume, Math.Floor(maxPositionBudget / adjustedBuyPrice));
        if (volume < 1)
        {
            return;
        }

        payload.Portfolio.Balance -= adjustedBuyPrice * volume;
        payload.Portfolio.OpenPositions[candidate.Symbol.Id] = new OpenPosition(
            candidate.Symbol.Id,
            candidate.Symbol.Name,
            candidate.Symbol.Subcode,
            adjustedBuyPrice,
            volume,
            currentDate
        );
    }

    private decimal ClampToSymbolLimit(
        ScoredSymbol candidate,
        decimal volume,
        decimal limitWindowsPerDay
    )
    {
        var symbolLimit = candidate.Symbol.AdditionalFields.Limit;
        if (!symbolLimit.HasValue)
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

        return Math.Min(volume, Math.Floor(symbolLimit.Value * limitWindowsPerDay));
    }
}
