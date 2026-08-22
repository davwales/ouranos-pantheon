using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;

public sealed class IterationSetupStep(IDbContextFactory<PlutusDbContext> dbContextFactory)
    : IStep<BacktestPayload>
{
    private const int MinimumProgressUpdate = 1;

    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory = Guard.Against.Null(
        dbContextFactory
    );
    private int _lastSavedPercent;

    public async Task ExecuteAsync(PipelineContext context, BacktestPayload payload)
    {
        if (payload.Entity is null || payload.ProgressInterval <= 0)
        {
            return;
        }

        if (
            context.CurrentIteration % payload.ProgressInterval != 0
            && context.CurrentIteration != context.TotalIterations - 1
        )
        {
            return;
        }

        var percent = 10 + (int)(80.0 * context.CurrentIteration / context.TotalIterations);

        if (percent - _lastSavedPercent < MinimumProgressUpdate)
        {
            return;
        }

        _lastSavedPercent = percent;

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
            CancellationToken.None
        );

        var currentStatus = await dbContext
            .Backtests.AsNoTracking()
            .Where(b => b.Id == payload.Entity.Id)
            .Select(b => b.Status)
            .FirstOrDefaultAsync(CancellationToken.None);

        if (currentStatus == BacktestStatus.Cancelled)
        {
            throw new OperationCanceledException($"Backtest '{payload.Entity.Id}' was cancelled.");
        }

        payload.Entity.UpdateProgress(
            percent,
            $"Simulating day {context.CurrentIteration} of {payload.Parameters.TotalDays}..."
        );

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
