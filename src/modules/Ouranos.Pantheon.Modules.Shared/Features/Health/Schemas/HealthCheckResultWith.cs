namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

internal sealed record HealthCheckResultWith(
    string Name,
    HealthStatus Status,
    string Description,
    DateTime Timestamp,
    object? Data = null
);
