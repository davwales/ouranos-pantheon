namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

public sealed record HealthCheckResult(
    HealthStatus Status,
    string Description,
    DateTime Timestamp,
    object? Data = null
);
