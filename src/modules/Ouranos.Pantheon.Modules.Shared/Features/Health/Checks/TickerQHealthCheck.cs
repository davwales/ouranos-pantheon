using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Enums;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;

public sealed class TickerQHealthCheck(
    IServiceScopeFactory scopeFactory,
    ILogger<TickerQHealthCheck> logger
) : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory = Guard.Against.Null(scopeFactory);
    private readonly ILogger<TickerQHealthCheck> _logger = Guard.Against.Null(logger);

    public string Name => "tickerQ";

    public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Checking TickerQ health.");

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<
            IDbContextFactory<TickerQDbContext>
        >();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var enabledTickers = await dbContext
            .Set<CronTickerEntity>()
            .Where(t => t.IsEnabled)
            .ToListAsync(cancellationToken);

        if (enabledTickers.Count == 0)
        {
            var zeroData = new TickerQHealthData(0, 0, 0, 0, 0);

            return new HealthCheckResult(
                HealthStatus.NotConfigured,
                "No enabled tickers found",
                DateTime.UtcNow,
                zeroData
            );
        }

        var overallStatus = HealthStatus.Healthy;
        var details = new List<string>();
        var healthy = 0;
        var failed = 0;
        var overdue = 0;
        var neverRan = 0;

        foreach (var ticker in enabledTickers)
        {
            var occurrences = await dbContext
                .Set<CronTickerOccurrenceEntity<CronTickerEntity>>()
                .Where(o => o.CronTickerId == ticker.Id)
                .OrderByDescending(o => o.ExecutionTime)
                .Take(5)
                .ToListAsync(cancellationToken);

            if (occurrences.Count == 0)
            {
                overallStatus = WorstOf(overallStatus, HealthStatus.Degraded);
                details.Add($"{ticker.Description ?? ticker.Function}: never ran");
                neverRan++;
                continue;
            }

            var latest = occurrences.First();

            if (!string.IsNullOrWhiteSpace(latest.ExceptionMessage))
            {
                overallStatus = WorstOf(overallStatus, HealthStatus.Unhealthy);
                details.Add(
                    $"{ticker.Description ?? ticker.Function}: last run failed — {latest.ExceptionMessage}"
                );
                failed++;
                continue;
            }

            if (
                latest.Status != TickerStatus.Done
                && latest.Status != TickerStatus.DueDone
                && IsOverdue(ticker.Expression, latest.ExecutionTime)
            )
            {
                overallStatus = WorstOf(overallStatus, HealthStatus.Degraded);
                details.Add(
                    $"{ticker.Description ?? ticker.Function}: overdue (status {latest.Status})"
                );
                overdue++;
                continue;
            }

            details.Add($"{ticker.Description ?? ticker.Function}: healthy");
            healthy++;
        }

        var description = string.Join("; ", details);

        var data = new TickerQHealthData(
            Healthy: healthy,
            Failed: failed,
            Overdue: overdue,
            NeverRan: neverRan,
            Total: enabledTickers.Count
        );

        _logger.LogDebug(
            "TickerQ health check result: {Status} — {Description}.",
            overallStatus,
            description
        );

        return new HealthCheckResult(overallStatus, description, DateTime.UtcNow, data);
    }

    private static HealthStatus WorstOf(HealthStatus current, HealthStatus candidate)
    {
        if (candidate == HealthStatus.Unhealthy)
        {
            return HealthStatus.Unhealthy;
        }

        if (current != HealthStatus.Unhealthy && candidate == HealthStatus.Degraded)
        {
            return HealthStatus.Degraded;
        }

        return current;
    }

    private bool IsOverdue(string cronExpression, DateTime lastExecutionTime)
    {
        try
        {
            var parts = cronExpression.Split(' ');
            var minutePart = parts[0];

            if (minutePart.Equals("*", StringComparison.Ordinal))
            {
                var threshold = TimeSpan.FromMinutes(2);
                return DateTime.UtcNow - lastExecutionTime > threshold;
            }

            if (int.TryParse(minutePart, out var minute))
            {
                var threshold = TimeSpan.FromHours(2);
                return DateTime.UtcNow - lastExecutionTime > threshold;
            }

            var defaultThreshold = TimeSpan.FromHours(2);
            return DateTime.UtcNow - lastExecutionTime > defaultThreshold;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to parse cron expression: {CronExpression}",
                cronExpression
            );
            return false;
        }
    }
}
