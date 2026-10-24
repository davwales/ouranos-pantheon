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
) : IHealthCheck, IAsyncDisposable
{
    private readonly IOptions<RabbitMqOptions> _options = Guard.Against.Null(options);
    private readonly ILogger<RabbitMqHealthCheck> _logger = Guard.Against.Null(logger);
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

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
            await EnsureConnectedAsync(opts, cancellationToken);

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

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }

    // Probes run every few seconds; a connection per probe churns the broker's connection log.
    private async Task EnsureConnectedAsync(
        RabbitMqOptions opts,
        CancellationToken cancellationToken
    )
    {
        if (_connection is { IsOpen: true })
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
            {
                return;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            var factory = new ConnectionFactory
            {
                HostName = opts.Host,
                UserName = opts.Username,
                Password = opts.Password,
                ClientProvidedName = "pantheon-health",
                // A closed connection must surface as Unhealthy, not be hidden by background recovery.
                AutomaticRecoveryEnabled = false,
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}
