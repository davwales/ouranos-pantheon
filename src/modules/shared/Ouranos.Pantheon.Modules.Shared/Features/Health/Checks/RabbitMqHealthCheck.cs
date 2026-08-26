using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Ouranos.Pantheon.Modules.Shared.Infra.RabbitMq;
using RabbitMQ.Client;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;

public sealed class RabbitMqHealthCheck(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqHealthCheck> logger
) : IHealthCheck
{
    private readonly IOptions<RabbitMqOptions> _options = Guard.Against.Null(options);
    private readonly ILogger<RabbitMqHealthCheck> _logger = Guard.Against.Null(logger);

    public string Name => "rabbitmq";

    public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Checking RabbitMQ health.");

        var opts = _options.Value;

        if (string.IsNullOrWhiteSpace(opts.Host))
        {
            return new HealthCheckResult(
                HealthStatus.NotConfigured,
                "RabbitMQ is not configured",
                DateTime.UtcNow
            );
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = opts.Host,
                UserName = opts.Username,
                Password = opts.Password,
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);

            var description = $"Connected to RabbitMQ at {opts.Host}:5672";

            _logger.LogDebug("RabbitMQ health check passed: {Description}.", description);

            return new HealthCheckResult(HealthStatus.Healthy, description, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ health check failed.");

            return new HealthCheckResult(HealthStatus.Unhealthy, ex.Message, DateTime.UtcNow);
        }
    }
}
