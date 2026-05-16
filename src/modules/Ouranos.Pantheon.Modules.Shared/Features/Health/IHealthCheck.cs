namespace Ouranos.Pantheon.Modules.Shared.Features.Health;

using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;

public interface IHealthCheck
{
    string Name { get; }
    Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken);
}
