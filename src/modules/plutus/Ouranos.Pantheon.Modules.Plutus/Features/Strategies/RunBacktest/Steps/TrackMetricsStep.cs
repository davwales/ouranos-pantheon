using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class TrackMetricsStep : IStep<BacktestPayload>
{
    public Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        Guard.Against.Null(payload.Context);

        var ctx = payload.Context;
        var currentDate = ctx.CurrentDate(context);

        var openPositionValue = payload.Portfolio.OpenPositions.Values
            .Sum(p => ctx.Data.GetLatestPrice(p.SymbolId, currentDate) * p.Volume);

        var currentPortfolioValue = payload.Portfolio.Balance + openPositionValue;

        payload.Portfolio.PeakPortfolioValue = Math.Max(payload.Portfolio.PeakPortfolioValue, currentPortfolioValue);

        var drawdown = payload.Portfolio.PeakPortfolioValue > 0
            ? (payload.Portfolio.PeakPortfolioValue - currentPortfolioValue) / payload.Portfolio.PeakPortfolioValue
            : 0m;

        payload.Portfolio.MaxDrawdown = Math.Max(payload.Portfolio.MaxDrawdown, drawdown);
        payload.Portfolio.PortfolioValues.Add(currentPortfolioValue);

        return Task.CompletedTask;
    }
}
