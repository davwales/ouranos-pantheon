using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;

public sealed class PostgresHealthCheck(
    IOptions<PostgresOptions> options,
    ILogger<PostgresHealthCheck> logger
) : IHealthCheck
{
    private readonly IOptions<PostgresOptions> _options = Guard.Against.Null(options);
    private readonly ILogger<PostgresHealthCheck> _logger = Guard.Against.Null(logger);

    public string Name => "postgres";

    public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Checking PostgreSQL health.");

        var opts = _options.Value;

        if (string.IsNullOrWhiteSpace(opts.Host))
        {
            return new HealthCheckResult(
                HealthStatus.NotConfigured,
                "PostgreSQL is not configured",
                DateTime.UtcNow
            );
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await using var connection = new NpgsqlConnection(opts.GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            stopwatch.Stop();
            var description = $"Connected to PostgreSQL ({stopwatch.ElapsedMilliseconds}ms)";

            _logger.LogDebug("PostgreSQL health check passed: {Description}.", description);

            return new HealthCheckResult(HealthStatus.Healthy, description, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostgreSQL health check failed.");

            return new HealthCheckResult(HealthStatus.Unhealthy, ex.Message, DateTime.UtcNow);
        }
    }
}
