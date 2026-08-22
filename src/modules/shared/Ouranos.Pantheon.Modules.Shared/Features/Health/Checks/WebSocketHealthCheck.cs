using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Contract.WebSockets;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;

public sealed class WebSocketHealthCheck(
    WebSocketHealthState healthState,
    ILogger<WebSocketHealthCheck> logger
) : IHealthCheck
{
    private readonly WebSocketHealthState _healthState = Guard.Against.Null(healthState);
    private readonly ILogger<WebSocketHealthCheck> _logger = Guard.Against.Null(logger);

    public string Name => "websockets";

    public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Checking WebSocket health.");

        var connections = _healthState.GetConnections();

        if (connections.Count == 0)
        {
            return Task.FromResult(
                new HealthCheckResult(
                    HealthStatus.NotConfigured,
                    "No WebSocket workers registered",
                    DateTime.UtcNow
                )
            );
        }

        var connected = connections.Where(c => c.Value).ToList();
        var disconnected = connections.Where(c => !c.Value).ToList();

        if (disconnected.Count == 0)
        {
            var description =
                $"All WebSocket workers connected: {string.Join(", ", connected.Select(c => c.Key))}";

            _logger.LogDebug("WebSocket health check passed: {Description}.", description);

            return Task.FromResult(
                new HealthCheckResult(HealthStatus.Healthy, description, DateTime.UtcNow)
            );
        }

        if (connected.Count == 0)
        {
            _logger.LogError("All WebSocket connections are down.");

            return Task.FromResult(
                new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    "All WebSocket connections are down",
                    DateTime.UtcNow
                )
            );
        }

        var degradedDescription =
            $"Connected: {string.Join(", ", connected.Select(c => c.Key))}. "
            + $"Disconnected: {string.Join(", ", disconnected.Select(c => c.Key))}";

        _logger.LogWarning("WebSocket health check degraded: {Description}.", degradedDescription);

        return Task.FromResult(
            new HealthCheckResult(HealthStatus.Degraded, degradedDescription, DateTime.UtcNow)
        );
    }
}
