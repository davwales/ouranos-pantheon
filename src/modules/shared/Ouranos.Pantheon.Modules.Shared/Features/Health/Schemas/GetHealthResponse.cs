namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

public sealed record GetHealthResponse(
    HealthStatus Status,
    Dictionary<string, HealthCheckResult> Checks
);
