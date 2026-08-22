using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health;

public sealed class GetHealthHandler : IPantheonHandler<GetHealthRequest, GetHealthResponse>
{
    private readonly IEnumerable<IHealthCheck> _checks;
    private readonly ILogger<GetHealthHandler> _logger;
    private readonly IOptions<HealthOptions> _options;

    public GetHealthHandler(
        ILogger<GetHealthHandler> logger,
        IEnumerable<IHealthCheck> checks,
        IOptions<HealthOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(checks);
        Guard.Against.Null(options);

        _logger = logger;
        _checks = checks;
        _options = options;
    }

    public async Task<GetHealthResponse> Handle(
        GetHealthRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to run health checks.");
        cancellationToken.ThrowIfCancellationRequested();

        var timeoutSeconds = _options.Value.PerCheckTimeoutSeconds;
        var checkTasks = _checks.Select(check =>
            RunCheckAsync(check, timeoutSeconds, cancellationToken)
        );

        var results = await Task.WhenAll(checkTasks);

        var checksDict = results.ToDictionary(
            r => r.Name,
            r => new HealthCheckResult(r.Status, r.Description, r.Timestamp, r.Data)
        );

        var overallStatus = ComputeOverallStatus(checksDict.Values);

        _logger.LogDebug("Health check completed: {Status}.", overallStatus);

        return new GetHealthResponse(overallStatus, checksDict);
    }

    private async Task<HealthCheckResultWith> RunCheckAsync(
        IHealthCheck check,
        int timeoutSeconds,
        CancellationToken cancellationToken
    )
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var result = await check.CheckAsync(cts.Token);
            return new HealthCheckResultWith(
                check.Name,
                result.Status,
                result.Description,
                result.Timestamp,
                result.Data
            );
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Health check '{Name}' timed out after {Timeout}s.",
                check.Name,
                timeoutSeconds
            );

            return new HealthCheckResultWith(
                check.Name,
                HealthStatus.Unhealthy,
                $"Timed out after {timeoutSeconds}s",
                DateTime.UtcNow,
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check '{Name}' threw an exception.", check.Name);

            return new HealthCheckResultWith(
                check.Name,
                HealthStatus.Unhealthy,
                ex.Message,
                DateTime.UtcNow,
                null
            );
        }
    }

    private static HealthStatus ComputeOverallStatus(IEnumerable<HealthCheckResult> results)
    {
        var resultList = results.ToList();

        if (resultList.Count == 0)
        {
            return HealthStatus.Healthy;
        }

        if (resultList.Any(r => r.Status == HealthStatus.Unhealthy))
        {
            return HealthStatus.Unhealthy;
        }

        if (resultList.Any(r => r.Status == HealthStatus.Degraded))
        {
            return HealthStatus.Degraded;
        }

        if (resultList.All(r => r.Status == HealthStatus.NotConfigured))
        {
            return HealthStatus.NotConfigured;
        }

        return HealthStatus.Healthy;
    }
}
