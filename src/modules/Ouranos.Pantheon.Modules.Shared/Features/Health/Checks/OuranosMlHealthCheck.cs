using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Features.Health.Schemas;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Shared.Features.Health.Checks;

public sealed class OuranosMlHealthCheck(
    IOptions<OuranosMachineLearningOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<OuranosMlHealthCheck> logger
) : IHealthCheck
{
    private readonly IOptions<OuranosMachineLearningOptions> _options = Guard.Against.Null(options);
    private readonly IHttpClientFactory _httpClientFactory = Guard.Against.Null(httpClientFactory);
    private readonly ILogger<OuranosMlHealthCheck> _logger = Guard.Against.Null(logger);

    public string Name => "ouranosMl";

    public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Checking Ouranos ML health.");

        var connectionString = _options.Value.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new HealthCheckResult(
                HealthStatus.NotConfigured,
                "Ouranos ML is not configured",
                DateTime.UtcNow
            );
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{connectionString}/health", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Ouranos ML health check passed.");

                return new HealthCheckResult(
                    HealthStatus.Healthy,
                    "Ouranos ML is reachable",
                    DateTime.UtcNow
                );
            }

            var description =
                $"Ouranos ML returned {(int)response.StatusCode}: {response.ReasonPhrase}";

            _logger.LogWarning("Ouranos ML health check degraded: {Description}.", description);

            return new HealthCheckResult(HealthStatus.Unhealthy, description, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ouranos ML health check failed.");

            return new HealthCheckResult(HealthStatus.Unhealthy, ex.Message, DateTime.UtcNow);
        }
    }
}
